namespace ProductStock.Application.DTOs.Products;

// Yeni ürün ekleme "formu": kullanıcının (frontend'in) gönderdiği veri.
// Bilerek OLMAYAN alanlar:
//  - Id: veritabanı verir.
//  - StockQuantity: ürün 0 stokla oluşur. Stok sadece "stok girişi" işlemiyle artar,
//    böylece her stok değişikliği stok hareketi olarak kayıt altında olur.
//  - CreatedAt/UpdatedAt: sunucu kendisi yazar, kullanıcıya bırakılmaz.
// ": IProductFields" → bu form "ürün alanları" sözleşmesini imzalar (ortak doğrulama kuralları için).
public class CreateProductRequest : IProductFields
{
    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CriticalStockLevel { get; set; }
}
