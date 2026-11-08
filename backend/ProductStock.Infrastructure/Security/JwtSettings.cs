namespace ProductStock.Infrastructure.Security;

// appsettings.json'daki "JwtSettings" bölümü. SecretKey gizlidir → User Secrets'ta durur.
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    // Bilekliği kim verdi? (Bizim API'miz)
    public string Issuer { get; set; } = string.Empty;

    // Bileklik kim için? (Bizim frontend'imiz)
    public string Audience { get; set; } = string.Empty;

    // Mühür anahtarı. Sadece sunucu bilir; bilen herkes sahte bileklik yapabilir → ASLA koda/git'e girmez.
    // HMAC-SHA256 için en az 32 bayt (karakter) olmalı.
    public string SecretKey { get; set; } = string.Empty;

    // Bilekliğin geçerlilik süresi (dakika)
    public int ExpiryMinutes { get; set; } = 60;
}
