using ProductStock.Application.DTOs.Auth;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;

namespace ProductStock.Application.Services;

// Giriş "müdürü": kimliği kontrol eder, doğruysa bileklik (token) verir.
public class AuthService : IAuthService
{
    // Kullanıcı adı yanlış da olsa şifre yanlış da olsa AYNI mesaj.
    // Farklı mesaj verseydik, kötü niyetli biri önce hangi kullanıcı adlarının var olduğunu öğrenir,
    // sonra sadece onların şifrelerini denerdi ("bu kapı doğru, anahtar yanlış" demek gibi).
    private const string InvalidCredentialsMessage = "Kullanıcı adı veya şifre hatalı.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            throw new UnauthorizedException(InvalidCredentialsMessage);

        // Kullanıcı adları küçük harfle saklanıyor: "Admin" = "admin"
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await _users.GetByUsernameAsync(username, cancellationToken);

        // Kullanıcı yok, pasif ya da şifre yanlış → hepsinde aynı cevap.
        if (user is null || !user.IsActive || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException(InvalidCredentialsMessage);

        // Geçiş planı: güvenlik damgası özelliğinden ÖNCE oluşturulan kullanıcıların damgası yok.
        // İlk girişlerinde damga üretip kaydediyoruz; böylece veritabanını elle düzeltmeye gerek kalmıyor.
        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            user.SecurityStamp = User.NewSecurityStamp();
            await _users.SetSecurityStampAsync(user.Id, user.SecurityStamp, cancellationToken);
        }

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
}
