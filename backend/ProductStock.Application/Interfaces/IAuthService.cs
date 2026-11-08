using ProductStock.Application.DTOs.Auth;

namespace ProductStock.Application.Interfaces;

// Giriş "müdürü" sözleşmesi.
public interface IAuthService
{
    // Kullanıcı adı + şifre doğruysa token döner; değilse UnauthorizedException fırlatır.
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
