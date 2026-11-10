using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Application.DTOs.Users;

// Dışarıya gösterilen kullanıcı bilgisi.
// DİKKAT: PasswordHash burada YOK. Şifrenin kıyması bile dışarı çıkmamalı.
// Entity'yi (User) doğrudan döndürseydik PasswordHash de cevaba girerdi; DTO kullanmamızın bir sebebi bu.
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Entity → DTO çevirici. Hangi alanların dışarı çıkacağına tek yerde karar veriyoruz.
    public static UserResponse FromEntity(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
