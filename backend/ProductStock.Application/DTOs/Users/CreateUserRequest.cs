using ProductStock.Domain.Enums;

namespace ProductStock.Application.DTOs.Users;

// Admin'in yeni kullanıcı eklerken dolduracağı form.
public class CreateUserRequest
{
    // Giriş adı. Küçük harfe çevrilip saklanır. Sadece a-z, 0-9, nokta, alt çizgi, tire.
    public string Username { get; set; } = string.Empty;

    // Ekranda görünen ad, ör. "Ayşe Yılmaz"
    public string FullName { get; set; } = string.Empty;

    // İlk şifre (en az 8 karakter). Kullanıcı ileride kendisi değiştirebilecek.
    public string Password { get; set; } = string.Empty;

    // "Admin" veya "WarehouseStaff" (JSON'da metin olarak yazılır).
    public UserRole Role { get; set; }
}
