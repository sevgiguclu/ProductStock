using ProductStock.Domain.Entities;

namespace ProductStock.Application.DTOs.Products;

// Kullanıcıya geri gönderilen ürün bilgisi ("hesap özeti").
// Entity'yi doğrudan dışarı açmıyoruz; dışarıya ne gideceğine burada biz karar veriyoruz.
public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }

    // Hesaplanmış alan: veritabanında saklanmaz, her seferinde stoktan hesaplanır.
    // Ekranda "kritik stok" uyarısı göstermek için kullanılacak.
    public bool IsCriticalStock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // static: bu metodu çağırmak için önce bir ProductResponse oluşturmaya gerek yok.
    // Kullanımı: ProductResponse.FromEntity(urun)
    // Entity → DTO çevirisini tek bir yerde yapıyoruz; her yerde tekrar yazmayalım.
    public static ProductResponse FromEntity(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Sku = product.Sku,
        Description = product.Description,
        Category = product.Category,
        Price = product.Price,
        StockQuantity = product.StockQuantity,
        CriticalStockLevel = product.CriticalStockLevel,
        IsCriticalStock = product.StockQuantity <= product.CriticalStockLevel,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
