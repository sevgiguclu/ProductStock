namespace ProductStock.Infrastructure.Security;

// appsettings.json'daki "SeedAdmin" bölümü: veritabanında hiç kullanıcı yokken oluşturulacak ilk Admin.
// Password gizlidir → User Secrets'ta durur.
public class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";

    public string Username { get; set; } = "admin";
    public string FullName { get; set; } = "Sistem Yöneticisi";
    public string Password { get; set; } = string.Empty;
}
