using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductStock.Application.DTOs.Products;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Infrastructure.Repositories;

// IProductRepository sözleşmesinin MongoDB ile yazılmış hali.
// ": IProductRepository" → "bu sınıf o sözleşmedeki tüm metotları yerine getirir" demek.
public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    // Constructor: MongoDbContext'i DI'dan alıp "products" koleksiyonuna erişimi hazırlıyoruz.
    public ProductRepository(MongoDbContext context)
    {
        _products = context.Database.GetCollection<Product>(MongoCollectionNames.Products);
    }

    public async Task<(List<Product> Items, long TotalCount)> SearchAsync(ProductQueryParameters query, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(query);

        // Toplam sayı: sayfa düğmeleri için filtreye uyan TÜM ürünlerin sayısı.
        var totalCount = await _products.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Sayfalama: Skip = "baştan şu kadarını atla", Limit = "şu kadarını al".
        // Örn. 3. sayfa, sayfada 20 → ilk 40'ı atla, sonraki 20'yi al.
        var items = await _products
            .Find(filter)
            .Sort(BuildSort(query))
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // Fişteki tercihlerden MongoDB filtresi oluşturur. Boş bırakılan tercih eklenmez.
    private static FilterDefinition<Product> BuildFilter(ProductQueryParameters query)
    {
        var f = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Regex.Escape: kullanıcının yazdığı "(" veya "." gibi özel karakterleri sıradan harfe çevirir.
            // Yoksa "(" yazan biri hatalı bir arama kalıbı oluşturup hataya sebep olurdu.
            // "i" → büyük-küçük harf fark etmesin.
            var pattern = new BsonRegularExpression(Regex.Escape(query.Search.Trim()), "i");

            // Adında VEYA SKU'sunda geçenler
            filters.Add(f.Or(f.Regex(p => p.Name, pattern), f.Regex(p => p.Sku, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
            filters.Add(f.Eq(p => p.Category, query.Category.Trim()));

        // Gte = büyük veya eşit (>=), Lte = küçük veya eşit (<=)
        if (query.MinPrice.HasValue)
            filters.Add(f.Gte(p => p.Price, query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            filters.Add(f.Lte(p => p.Price, query.MaxPrice.Value));

        switch (query.StockStatus)
        {
            case StockStatusFilter.OutOfStock:
                filters.Add(f.Eq(p => p.StockQuantity, 0));
                break;
            case StockStatusFilter.Critical:
                // İki alanı birbiriyle karşılaştırıyoruz (stok <= kritik seviye).
                filters.Add(f.Where(p => p.StockQuantity <= p.CriticalStockLevel));
                break;
            case StockStatusFilter.Sufficient:
                filters.Add(f.Where(p => p.StockQuantity > p.CriticalStockLevel));
                break;
        }

        // Hiç tercih yoksa "filtre yok" (hepsi); varsa hepsi birlikte (VE) uygulanır.
        return filters.Count == 0 ? f.Empty : f.And(filters);
    }

    private static SortDefinition<Product> BuildSort(ProductQueryParameters query)
    {
        var s = Builders<Product>.Sort;
        var descending = query.SortDescending;

        var primary = query.SortBy switch
        {
            ProductSortBy.Price => descending ? s.Descending(p => p.Price) : s.Ascending(p => p.Price),
            ProductSortBy.StockQuantity => descending ? s.Descending(p => p.StockQuantity) : s.Ascending(p => p.StockQuantity),
            ProductSortBy.CreatedAt => descending ? s.Descending(p => p.CreatedAt) : s.Ascending(p => p.CreatedAt),
            _ => descending ? s.Descending(p => p.Name) : s.Ascending(p => p.Name)
        };

        // İkinci sıralama Id'ye göre: fiyatı aynı olan iki ürün sayfalar arasında yer değiştirmesin
        // (aynı ürün iki sayfada birden görünmesin ya da hiçbir sayfada görünmeden kaybolmasın).
        return s.Combine(primary, s.Ascending(p => p.Id));
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
        try
        {
            // product.Id boş olduğu için MongoMappings'teki kural yeni bir ObjectId üretip product.Id'ye yazar.
            await _products.InsertOneAsync(product, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (IsDuplicateKey(ex))
        {
            // SKU benzersiz index'i ikinci kaydı reddetti (ör. aynı anda iki istek geldi).
            // MongoDB'nin hatasını bizim tanıdığımız alarma çeviriyoruz → kullanıcıya 409 döner.
            throw new ConflictException($"'{product.Sku}' stok kodlu bir ürün zaten var.");
        }
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

        try
        {
            var result = await _products.UpdateOneAsync(p => p.Id == product.Id, update, cancellationToken: cancellationToken);

            // MatchedCount: filtreye uyan (bulunan) belge sayısı. 0 ise ürün yok demektir.
            return result.MatchedCount > 0;
        }
        catch (MongoWriteException ex) when (IsDuplicateKey(ex))
        {
            throw new ConflictException($"'{product.Sku}' stok kodlu başka bir ürün zaten var.");
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _products.DeleteOneAsync(p => p.Id == id, cancellationToken);

        // DeletedCount: silinen belge sayısı. 0 ise silinecek ürün bulunamamıştır.
        return result.DeletedCount > 0;
    }

    // MongoDB hatası "aynı değer zaten var" (duplicate key, kod 11000) türünde mi?
    private static bool IsDuplicateKey(MongoWriteException ex) =>
        ex.WriteError?.Category == ServerErrorCategory.DuplicateKey;
}
