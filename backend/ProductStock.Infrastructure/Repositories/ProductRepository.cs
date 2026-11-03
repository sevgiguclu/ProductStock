using MongoDB.Bson;
using MongoDB.Driver;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Infrastructure.Repositories;

// IProductRepository sözleşmesinin MongoDB ile yazılmış hali.
// ": IProductRepository" → "bu sınıf o sözleşmedeki tüm metotları yerine getirir" demek.
public class ProductRepository : IProductRepository
{
    // MongoDB'de ürünlerin durduğu koleksiyonun ("klasör") adı.
    private const string CollectionName = "products";

    private readonly IMongoCollection<Product> _products;

    // Constructor: MongoDbContext'i DI'dan alıp "products" koleksiyonuna erişimi hazırlıyoruz.
    public ProductRepository(MongoDbContext context)
    {
        _products = context.Database.GetCollection<Product>(CollectionName);
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Filtre yok (Empty) → hepsini getir; ada göre alfabetik sırala.
        return await _products
            .Find(FilterDefinition<Product>.Empty)
            .SortBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Id, ObjectId biçiminde değilse (ör. "abc") veritabanına hiç sormadan "yok" diyoruz.
        // Aksi halde sürücü metni ObjectId'ye çeviremeyip hata fırlatırdı.
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        // Limit = 1: bir tane bulmak yeterli, hepsini saymaya gerek yok.
        var count = await _products.CountDocumentsAsync(
            p => p.Sku == sku,
            new CountOptions { Limit = 1 },
            cancellationToken);

        return count > 0;
    }

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        // product.Id boş olduğu için MongoMappings'teki kural yeni bir ObjectId üretip product.Id'ye yazar.
        await _products.InsertOneAsync(product, cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(product.Id, out _))
            return false;

        // Belgenin tamamını değiştirmek yerine sadece bu alanları "set" ediyoruz.
        // StockQuantity bilerek listede YOK: başka biri aynı anda stok çıkışı yaparsa
        // onun değişikliğini ezmemek için stok sadece stok işlemleriyle değişir.
        var update = Builders<Product>.Update
            .Set(p => p.Name, product.Name)
            .Set(p => p.Sku, product.Sku)
            .Set(p => p.Description, product.Description)
            .Set(p => p.Category, product.Category)
            .Set(p => p.Price, product.Price)
            .Set(p => p.CriticalStockLevel, product.CriticalStockLevel)
            .Set(p => p.UpdatedAt, product.UpdatedAt);

        var result = await _products.UpdateOneAsync(p => p.Id == product.Id, update, cancellationToken: cancellationToken);

        // MatchedCount: filtreye uyan (bulunan) belge sayısı. 0 ise ürün yok demektir.
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _products.DeleteOneAsync(p => p.Id == id, cancellationToken);

        // DeletedCount: silinen belge sayısı. 0 ise silinecek ürün bulunamamıştır.
        return result.DeletedCount > 0;
    }
}
