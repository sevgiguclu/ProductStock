using ProductStock.Domain.Entities;

namespace ProductStock.Application.Interfaces;

// Stok hareketleri (fişler) için eleman sözleşmesi.
public interface IStockMovementRepository
{
    // Fişi uygular: ürünün stoğunu değiştirir VE fişi kaydeder, ikisini TEK işlem bütünü (transaction) olarak.
    // Biri başarısız olursa ikisi de geri alınır ("ya hep ya hiç").
    // Çıkışta stok yetersizse ya da ürün yoksa hiçbir şey değişmez ve null döner.
    // Başarılıysa ürünün YENİ halini döner; movement.StockAfter, ProductSku ve ProductName de doldurulur.
    Task<Product?> ApplyAsync(StockMovement movement, CancellationToken cancellationToken = default);

    // Bir ürünün fişleri, en yeni önce, sayfalı.
    Task<(List<StockMovement> Items, long TotalCount)> GetByProductAsync(
        string productId, int page, int pageSize, CancellationToken cancellationToken = default);
}
