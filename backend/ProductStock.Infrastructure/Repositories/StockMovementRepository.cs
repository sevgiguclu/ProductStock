using MongoDB.Bson;
using MongoDB.Driver;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Infrastructure.Repositories;

// Stok fişlerini MongoDB ile uygulayan eleman.
public class StockMovementRepository : IStockMovementRepository
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<StockMovement> _movements;

    public StockMovementRepository(MongoDbContext context)
    {
        _client = context.Client;
        _products = context.Database.GetCollection<Product>(MongoCollectionNames.Products);
        _movements = context.Database.GetCollection<StockMovement>(MongoCollectionNames.StockMovements);
    }

    public async Task<Product?> ApplyAsync(StockMovement movement, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(movement.ProductId, out _))
            return null;

        // Giriş → stok artar (+), çıkış → stok azalır (−)
        var delta = movement.Type == StockMovementType.In ? movement.Quantity : -movement.Quantity;

        // KONTROL + DEĞİŞİKLİK TEK KOMUTTA (atomik):
        // "Id'si bu olan [çıkışsa: VE stoğu en az Quantity olan] ürünü bul, stoğunu delta kadar değiştir."
        // Stok yetersizse koşul tutmaz → hiçbir belge bulunmaz → hiçbir şey değişmez. Stok asla eksiye düşmez.
        // Aynı anda gelen iki çıkış isteği bile bu koşulu atlatamaz; MongoDB tek belgedeki bu işlemi bölmeden yapar.
        var filter = Builders<Product>.Filter.Eq(p => p.Id, movement.ProductId);
        if (delta < 0)
            filter &= Builders<Product>.Filter.Gte(p => p.StockQuantity, movement.Quantity);

        // $inc: alanı verilen miktar kadar artırır (negatifse azaltır).
        var update = Builders<Product>.Update.Inc(p => p.StockQuantity, delta);

        // ReturnDocument.After: güncellemeden SONRAKİ hali döndür (yeni stoğu öğrenmek için).
        var options = new FindOneAndUpdateOptions<Product> { ReturnDocument = ReturnDocument.After };

        // TRANSACTION ("ya hep ya hiç"): stok değişikliği ve fiş kaydı birlikte ya olur ya hiç olmaz.
        // Banka havalesi gibi: senden düşüp karşıya geçmezse para kaybolmasın, işlem geri alınsın.
        // "using": iş bitince session (oturum) otomatik kapatılsın.
        using var session = await _client.StartSessionAsync(cancellationToken: cancellationToken);

        // WithTransactionAsync: içindeki işleri transaction'da çalıştırır; geçici bir ağ hatasında
        // kendisi tekrar dener, hata kalıcıysa hepsini geri alır (abort), başarılıysa onaylar (commit).
        return await session.WithTransactionAsync<Product?>(async (s, token) =>
        {
            // İşlemlerde session'ı (s) veriyoruz ki ikisi de AYNI transaction'ın parçası olsun.
            var product = await _products.FindOneAndUpdateAsync(s, filter, update, options, token);
            if (product is null)
                return null;

            // Fişe işlem sonrası stoğu ve ürünün o anki SKU/adını yaz (anlık görüntü).
            movement.StockAfter = product.StockQuantity;
            movement.ProductSku = product.Sku;
            movement.ProductName = product.Name;

            await _movements.InsertOneAsync(s, movement, cancellationToken: token);

            return product;
        }, cancellationToken: cancellationToken);
    }

    public async Task<(List<StockMovement> Items, long TotalCount)> GetByProductAsync(
        string productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(productId, out _))
            return ([], 0);

        var filter = Builders<StockMovement>.Filter.Eq(m => m.ProductId, productId);

        var totalCount = await _movements.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _movements
            .Find(filter)
            // En yeni fiş en üstte; aynı anda oluşmuş iki fiş için Id ile sabit sıra.
            .Sort(Builders<StockMovement>.Sort.Descending(m => m.CreatedAt).Descending(m => m.Id))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
