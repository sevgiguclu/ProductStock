using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using ProductStock.Domain.Entities;

namespace ProductStock.Infrastructure.Persistence;

// C# sınıfları ile MongoDB belgeleri arasındaki "tercüman" kuralları.
// Domain'deki sınıflara MongoDB'ye özel etiket koymamak için kuralları burada, Infrastructure'da yazıyoruz.
// Bu kurallar uygulama başlarken BİR KEZ kaydedilmeli; iki kez kaydetmeye çalışmak hata verir.
public static class MongoMappings
{
    private static readonly object _lock = new();
    private static bool _registered;

    public static void Register()
    {
        // lock: aynı anda iki yerden çağrılırsa bile kayıt işini sadece biri yapsın.
        lock (_lock)
        {
            if (_registered)
                return;

            // Genel kurallar (convention): sadece bizim sınıflarımıza (ProductStock ile başlayan namespace) uygulanır.
            var conventions = new ConventionPack
            {
                // C#'taki "StockQuantity" → MongoDB'de "stockQuantity" olarak yazılır.
                new CamelCaseElementNameConvention(),
                // Veritabanında sınıfta olmayan bir alan varsa hata verme, görmezden gel.
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register(
                "ProductStockConventions",
                conventions,
                type => type.Namespace?.StartsWith("ProductStock") == true);

            BsonClassMap.RegisterClassMap<Product>(map =>
            {
                // AutoMap: tüm property'leri otomatik eşle; aşağıda sadece özel durumları değiştiriyoruz.
                map.AutoMap();

                // Id: C#'ta string, MongoDB'de ObjectId. Id boşsa kayıt sırasında yeni ObjectId üretilir.
                map.MapIdMember(p => p.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIdGenerator(StringObjectIdGenerator.Instance);

                // Fiyat: metin olarak değil, gerçek ondalık sayı (Decimal128) olarak saklansın;
                // böylece fiyat aralığı filtresi doğru çalışır.
                map.MapMember(p => p.Price)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });

            _registered = true;
        }
    }
}
