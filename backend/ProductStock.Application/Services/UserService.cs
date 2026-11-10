using System.Text.RegularExpressions;
using ProductStock.Application.DTOs.Users;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Application.Services;

// Kullanıcı yönetimi "müdürü": kuralları kontrol eder, şifreyi kıymadan geçirir, kaydeder.
public class UserService : IUserService
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    // Regex (düzenli ifade) = metin için kalıp.
    // ^ ... $          → metnin baştan sona tamamı bu kalıba uymalı
    // [a-z0-9._-]      → izin verilen karakterler: küçük harf, rakam, nokta, alt çizgi, tire
    // {3,32}           → en az 3, en fazla 32 karakter
    // Türkçe harf ve boşluk yok: giriş adı her klavyede sorunsuz yazılabilsin.
    private static readonly Regex UsernamePattern = new("^[a-z0-9._-]{3,32}$", RegexOptions.Compiled);

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
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
        // Girişte "Ayse.Yilmaz " yazılsa da "ayse.yilmaz" olarak saklansın (girişte de aynı çevrim yapılıyor).
        var username = (request.Username ?? string.Empty).Trim().ToLowerInvariant();
        var fullName = (request.FullName ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (!UsernamePattern.IsMatch(username))
            throw new BadRequestException(
                "Kullanıcı adı 3-32 karakter olmalı ve sadece küçük harf (a-z), rakam, nokta, alt çizgi, tire içermeli.");

        if (fullName.Length is 0 or > 100)
            throw new BadRequestException("Ad soyad boş olamaz ve en fazla 100 karakter olabilir.");

        // Şifrenin başındaki/sonundaki boşluklara dokunmuyoruz: boşluk da şifrenin parçası olabilir.
        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
            throw new BadRequestException($"Şifre en az {MinPasswordLength}, en fazla {MaxPasswordLength} karakter olmalı.");

        // Enum.IsDefined: gelen değer gerçekten tanımlı rollerden biri mi?
        // JSON'da "role": 5 gibi bir sayı gelirse metin çevirici bunu kabul eder; burada yakalıyoruz.
        if (!Enum.IsDefined(request.Role))
            throw new BadRequestException("Geçersiz rol. Geçerli değerler: Admin, WarehouseStaff.");

        var user = new User
        {
            Username = username,
            FullName = fullName,
            PasswordHash = _passwordHasher.Hash(password),   // şifrenin kendisi değil, kıyması saklanır
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Aynı kullanıcı adı varsa repository ConflictException (409) fırlatır.
        // Önce "var mı?" diye sormuyoruz: benzersiz index zaten koruyor ve yarış durumuna (race condition) karşı güvenli.
        await _users.CreateAsync(user, cancellationToken);

        return UserResponse.FromEntity(user);
    }
}
