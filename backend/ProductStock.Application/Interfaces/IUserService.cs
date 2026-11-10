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
}
