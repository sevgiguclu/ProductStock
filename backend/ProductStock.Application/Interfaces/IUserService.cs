using ProductStock.Application.DTOs.Auth;
using ProductStock.Application.DTOs.Users;

namespace ProductStock.Application.Interfaces;

// Kullanıcı yönetimi "müdürü" sözleşmesi (sadece Admin kullanır).
public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    // Yoksa NotFoundException (404).
    Task<UserResponse> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    // Kurallara uymazsa BadRequestException (400), kullanıcı adı alınmışsa ConflictException (409).
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    // Aktif/pasif yapar. Kendini pasif yapmaya çalışırsa 400, kullanıcı yoksa 404.
    Task<UserResponse> SetStatusAsync(string id, SetUserStatusRequest request, CancellationToken cancellationToken = default);

    // Giriş yapmış kullanıcının KENDİ şifresini değiştirir. Mevcut şifre yanlışsa 400.
    // Eski bileklikler geçersiz olur; kullanıcı atılmasın diye YENİ bileklik döner.
    Task<LoginResponse> ChangeOwnPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
