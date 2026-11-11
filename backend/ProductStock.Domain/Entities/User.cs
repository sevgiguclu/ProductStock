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

    // Güvenlik damgası ("günün rengi"): rastgele bir kod. Bileklik verilirken bileklige de kopyası yazılır.
    // Kullanıcı pasif yapılınca veya şifresi değişince damga YENİLENİR → eski damgalı bütün bileklikler geçersiz olur.
    // Bileklikler hiçbir yerde saklanmadığı için onları tek tek bulup silemeyiz; damgayı değiştirmek hepsini birden iptal eder.
    public string SecurityStamp { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    // Yeni rastgele damga üretir. Guid: tekrar etme ihtimali yok denecek kadar düşük rastgele kod.
    // "N" biçimi: tiresiz 32 karakter (ör. "3f9a1c0e...").
    public static string NewSecurityStamp() => Guid.NewGuid().ToString("N");
}
