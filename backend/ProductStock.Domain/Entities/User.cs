using ProductStock.Domain.Enums;

namespace ProductStock.Domain.Entities;

// Sisteme giriş yapabilen kullanıcı.
public class User
{
    public string Id { get; set; } = string.Empty;

    // Giriş adı. Her zaman küçük harfle saklanır ("Ayse" = "ayse"); benzersizdir.
    public string Username { get; set; } = string.Empty;

    // Ekranda görünen ad, ör. "Ayşe Yılmaz"
    public string FullName { get; set; } = string.Empty;

    // Şifrenin KENDİSİ DEĞİL, "hash"i (kıyması). Şifre asla olduğu gibi saklanmaz.
    // Veritabanı çalınsa bile buradan şifre geri elde edilemez.
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // Pasif kullanıcı giriş yapamaz (işten ayrılan çalışanı silmeden engellemek için).
    // Silmek yerine pasif yapıyoruz: geçmiş fişlerde "işlemi yapan" bilgisi anlamlı kalsın.
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
