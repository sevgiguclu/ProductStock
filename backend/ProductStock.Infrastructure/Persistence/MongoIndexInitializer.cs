using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProductStock.Domain.Entities;

namespace ProductStock.Infrastructure.Persistence;

// Uygulama açılırken BİR KEZ çalışır ve gerekli index'leri oluşturur (hosted service).
// Index zaten varsa MongoDB tekrar oluşturmaz; her açılışta çalışması sorun değildir.
public class MongoIndexInitializer : IHostedService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(MongoDbContext context, ILogger<MongoIndexInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Uygulama başlarken çağrılır.
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var products = _context.Database.GetCollection<Product>(MongoCollectionNames.Products);

        // SKU benzersiz index: aynı SKU ile iki ürün olmasını VERİTABANI düzeyinde engeller.
        // Aynı anda gelen iki istekte bile ikincisini MongoDB reddeder (son güvenlik kilidi).
        var skuIndex = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Sku),
            new CreateIndexOptions { Unique = true, Name = "ux_products_sku" });

        // Kategori index'i: kategoriye göre filtreleme hızlı olsun diye (benzersiz değil).
        var categoryIndex = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Category),
            new CreateIndexOptions { Name = "ix_products_category" });

        await products.Indexes.CreateManyAsync([skuIndex, categoryIndex], cancellationToken);

        _logger.LogInformation("MongoDB index'leri hazır.");
    }

    // Uygulama kapanırken çağrılır; burada yapacak bir işimiz yok.
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
