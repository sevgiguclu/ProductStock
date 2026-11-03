using ProductStock.Domain.Enums;

namespace ProductStock.Domain.Entities;

// Stok hareketi = stoktaki her değişikliğin "fişi": kim, ne zaman, hangi üründen, kaç tane, neden.
// Stok SADECE bu fişler kesilerek değişir; böylece her stoğun nereden geldiği kayıtlıdır.
public class StockMovement
{
    public string Id { get; set; } = string.Empty;

    // Hangi ürünün fişi (Product.Id)
    public string ProductId { get; set; } = string.Empty;

    // Ürünün o anki SKU'su ve adı ("anlık görüntü" / snapshot).
    // Ürün sonradan silinse ya da adı değişse bile eski fişte o gün hangi ürün olduğu okunabilir.
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    // Giriş mi, çıkış mı?
    public StockMovementType Type { get; set; }

    // Miktar her zaman POZİTİF yazılır; yönü Type söyler (In = +, Out = −).
    public int Quantity { get; set; }

    // İşlemden SONRA ürünün stoğu. Geçmişe bakarken "o an kaç tane vardı?" sorusunun cevabı.
    public int StockAfter { get; set; }

    // Neden: "Tedarikçiden alım", "Satış", "Fire" vb.
    public string Reason { get; set; } = string.Empty;

    // İşlemi yapan kullanıcı. JWT (5-6. gün) gelene kadar "sistem" yazılacak.
    public string PerformedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
