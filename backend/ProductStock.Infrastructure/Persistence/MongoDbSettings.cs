namespace ProductStock.Infrastructure.Persistence;

// appsettings.json'daki "MongoDbSettings" bölümünün C# karşılığı (Options pattern).
// Değerler appsettings.json'da boş durur; gerçek değerler geliştirmede User Secrets'tan,
// Docker/production'da environment variable'lardan (MongoDbSettings__ConnectionString) gelir.
public class MongoDbSettings
{
    // Bölüm adını sabit olarak tutuyoruz; Program.cs'te "MongoDbSettings" yazım hatası riskini önler.
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;
}
