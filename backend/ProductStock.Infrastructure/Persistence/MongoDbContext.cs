using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ProductStock.Infrastructure.Persistence;

// MongoDB bağlantısını kuran ve veritabanına erişim sağlayan sınıf.
// DI'a Singleton olarak kaydedilir: MongoClient kendi bağlantı havuzunu yönetir ve
// MongoDB'nin önerisi uygulama boyunca tek bir MongoClient kullanmaktır.
public class MongoDbContext
{
    private readonly ILogger<MongoDbContext> _logger;

    public IMongoDatabase Database { get; }

    // Constructor: ayarları IOptions<MongoDbSettings> olarak DI'dan alıyoruz.
    // IOptions, appsettings + User Secrets + environment variable katmanlarından
    // birleştirilmiş son değerleri taşır. ILogger da DI'dan gelir.
    public MongoDbContext(IOptions<MongoDbSettings> options, ILogger<MongoDbContext> logger)
    {
        _logger = logger;
        var settings = options.Value;

        // Ayar eksikse uygulamanın anlaşılmaz bir Mongo hatası yerine açık bir mesajla durmasını sağlıyoruz.
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            throw new InvalidOperationException(
                "MongoDbSettings:ConnectionString boş. User Secrets veya environment variable ile ayarlayın.");

        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
            throw new InvalidOperationException(
                "MongoDbSettings:DatabaseName boş. User Secrets veya environment variable ile ayarlayın.");

        var client = new MongoClient(settings.ConnectionString);
        Database = client.GetDatabase(settings.DatabaseName);
    }

    // Veritabanına "ping" komutu gönderir; bağlantı çalışıyorsa true döner.
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            // Hatayı yutmuyoruz; sebebini log'a yazıyoruz ki sorun teşhis edilebilsin.
            _logger.LogWarning(ex, "MongoDB ping başarısız oldu.");
            return false;
        }
    }
}
