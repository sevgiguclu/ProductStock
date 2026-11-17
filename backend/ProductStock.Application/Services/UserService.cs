using ProductStock.Application.DTOs.Auth;
using ProductStock.Application.DTOs.Users;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Application.Services;

// Kullanıcı yönetimi "müdürü": iş kurallarını uygular, şifreyi kıymadan geçirir, kaydeder.
// Form kuralları (kullanıcı adı kalıbı, şifre uzunluğu, rol geçerli mi...) burada DEĞİL:
// Validators/Users/UserValidators.cs'te. ValidationFilter, form buraya gelmeden ÖNCE onları çalıştırır.
// Burada sadece veritabanına/duruma bakmayı gerektiren kurallar var (mevcut şifre doğru mu, kendini pasif yapma...).
public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository users, IPasswordHasher passwordHasher, ICurrentUserService currentUser, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.GetAllAsync(cancellationToken);
        return users.Select(UserResponse.FromEntity).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"'{id}' Id'li kullanıcı bulunamadı.");

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Form kuralları CreateUserRequestValidator'da kontrol edildi. Burada sadece tek tipe getiriyoruz:
        // "Ayse.Yilmaz " → "ayse.yilmaz" (girişte de aynı çevrim yapılıyor).
        var user = new User
        {
            Username = request.Username.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),   // şifrenin kendisi değil, kıyması saklanır
            Role = request.Role,
            IsActive = true,
            SecurityStamp = User.NewSecurityStamp(),
            CreatedAt = DateTime.UtcNow
        };

        // Aynı kullanıcı adı varsa repository ConflictException (409) fırlatır.
        // Önce "var mı?" diye sormuyoruz: benzersiz index zaten koruyor ve yarış durumuna (race condition) karşı güvenli.
        await _users.CreateAsync(user, cancellationToken);

        return UserResponse.FromEntity(user);
    }

    public async Task<LoginResponse> ChangeOwnPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        // Kimin şifresi? Formdan DEĞİL, bileklikten (giriş yapan kişi). Böylece kimse başkasının şifresini değiştiremez.
        var username = _currentUser.Username;
        if (string.IsNullOrWhiteSpace(username))
            throw new UnauthorizedException("Kimlik doğrulanamadı.");

        var user = await _users.GetByUsernameAsync(username, cancellationToken)
            ?? throw new UnauthorizedException("Kimlik doğrulanamadı.");

        // Form kuralları (boş değil, yeni şifre 8-128, eskisiyle aynı değil) ChangePasswordRequestValidator'da.
        // Burada kalan İŞ KURALI: mevcut şifre gerçekten doğru mu? (veritabanındaki kıymaya bakmak gerekiyor)
        // Yanlışsa 401 DEĞİL 400: oturum geçerli, sadece form yanlış doldurulmuş.
        // 401 verseydik ön yüz "oturum bitti" sanıp kullanıcıyı giriş ekranına atardı.
        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
            throw new BadRequestException("Mevcut şifre hatalı.");

        // Şifreyle birlikte damga da yenilenir → bu kullanıcının BÜTÜN eski bileklikleri geçersiz olur
        // (başka bilgisayarda açık kalmış oturumlar da kapanır; şu an kullandığı bileklik de dahil).
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = User.NewSecurityStamp();
        await _users.UpdatePasswordHashAsync(user.Id, user.PasswordHash, user.SecurityStamp, cancellationToken);

        // Şifresini değiştiren kişi sistemden atılmasın diye ona YENİ damgalı yeni bir bileklik veriyoruz
        // (bankaların "şifren değişti, diğer cihazlardan çıkış yapıldı" davranışı).
        var (token, expiresAt) = _tokenService.CreateToken(user);
        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    public async Task<UserResponse> SetStatusAsync(string id, SetUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        // "isActive boş olamaz" kuralı SetUserStatusRequestValidator'da.
        // .Value: değer yoksa sessizce "false" (= pasif yap!) saymak yerine AÇIKÇA hata verir.
        // Normalde hiç olmaz (filtre engeller); filtre bir gün atlanırsa kullanıcı yanlışlıkla pasif yapılmasın diye.
        var isActive = request.IsActive!.Value;

        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"'{id}' Id'li kullanıcı bulunamadı.");

        // Kural: kimse kendi hesabını pasif yapamaz.
        // Tek Admin kendini kapatırsa sistemde yönetici kalmaz ve kimse onu geri açamaz.
        // İşlemi yapan hep aktif kaldığı için "en az bir aktif Admin" her zaman garanti.
        if (!isActive && user.Username == _currentUser.Username)
            throw new BadRequestException("Kendi hesabınızı pasif yapamazsınız.");

        // Durumla birlikte damga da yenilenir. Pasif yapınca: elindeki BÜTÜN bileklikler anında geçersiz.
        // (Aktif yapınca yenilemek şart değil ama zararı da yok: kişi zaten yeniden giriş yapacak.)
        await _users.SetActiveAsync(id, isActive, User.NewSecurityStamp(), cancellationToken);

        user.IsActive = isActive;
        return UserResponse.FromEntity(user);
    }
}
