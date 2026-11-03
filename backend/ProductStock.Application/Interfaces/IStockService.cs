using ProductStock.Application.DTOs.Common;
using ProductStock.Application.DTOs.Stock;

namespace ProductStock.Application.Interfaces;

// Stok işlemlerinin "müdür" sözleşmesi.
public interface IStockService
{
    // Stok girişi. Hatalı miktar/neden → BadRequestException, ürün yok → NotFoundException.
    Task<StockMovementResponse> StockInAsync(string productId, StockChangeRequest request, CancellationToken cancellationToken = default);

    // Stok çıkışı. Yukarıdakilere ek olarak stok yetersizse → ConflictException.
    Task<StockMovementResponse> StockOutAsync(string productId, StockChangeRequest request, CancellationToken cancellationToken = default);

    // Ürünün stok hareket geçmişi, en yeni önce. Ürün yok → NotFoundException.
    Task<PagedResult<StockMovementResponse>> GetMovementsAsync(string productId, PaginationParameters pagination, CancellationToken cancellationToken = default);
}
