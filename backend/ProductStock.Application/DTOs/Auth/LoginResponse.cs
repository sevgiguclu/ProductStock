using ProductStock.Domain.Enums;

namespace ProductStock.Application.DTOs.Auth;

// Başarılı giriş cevabı: "bileklik" (token) + ekranda göstermek için kullanıcı bilgileri.
public class LoginResponse
{
    // Sonraki her istekte "Authorization: Bearer <token>" başlığıyla gönderilecek metin.
    public string Token { get; set; } = string.Empty;

    // Bilekliğin son kullanma zamanı (UTC). Sonra tekrar giriş yapmak gerekir.
    public DateTime ExpiresAt { get; set; }

    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
