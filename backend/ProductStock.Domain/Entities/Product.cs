namespace ProductStock.Domain.Entities;

// Depodaki bir ürünü temsil eden entity.
// Domain katmanı hiçbir pakete bağlı değildir; bu yüzden burada MongoDB'ye özel
// attribute ([BsonId] vb.) kullanmıyoruz. Veritabanı eşlemesi Infrastructure katmanında yapılacak.
public class Product
{
    // MongoDB'nin ObjectId değerini string olarak tutuyoruz (ör. "671a3f..."),
    // böylece üst katmanlar MongoDB tiplerini tanımak zorunda kalmaz.
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    // SKU (Stock Keeping Unit): ürünü benzersiz tanımlayan stok kodu, ör. "KLV-001".
    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Category { get; set; } = string.Empty;

    // Para değerleri için double yerine decimal kullanılır; double yuvarlama hatası yapabilir.
    public decimal Price { get; set; }

    // Eldeki stok miktarı. Asla eksiye düşmemeli (stok işlemlerinde atomik kontrol edilecek).
    public int StockQuantity { get; set; }

    // Stok bu seviyenin altına inerse ürün "kritik stok" sayılır.
    public int CriticalStockLevel { get; set; }

    // Tarihler her zaman UTC tutulur; saat dilimine göre çevirme işi arayüzde yapılır.
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
