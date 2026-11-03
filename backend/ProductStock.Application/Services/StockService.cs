using ProductStock.Application.DTOs.Common;
using ProductStock.Application.DTOs.Stock;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Application.Services;

// Stok işlerinin "müdürü": fişin kurallara uygun olup olmadığına bakar, uygulamayı elemana bırakır.
public class StockService : IStockService
{
    // Tek fişte izin verilen en büyük miktar. int'in bir tavanı var (~2,1 milyar);
    // dev bir sayı stoğu taşırıp anlamsız bir değere çevirmesin.
    private const int MaxQuantity = 1_000_000;

    private const int MaxPageSize = 100;

    // JWT (5-6. gün) gelene kadar işlemi yapan olarak bu yazılır; sonra giriş yapan kullanıcının adı gelecek.
    private const string DefaultPerformedBy = "sistem";

    private readonly IStockMovementRepository _movements;
    private readonly IProductRepository _products;

    public StockService(IStockMovementRepository movements, IProductRepository products)
    {
        _movements = movements;
        _products = products;
    }

    public Task<StockMovementResponse> StockInAsync(string productId, StockChangeRequest request, CancellationToken cancellationToken = default)
        => ApplyAsync(productId, StockMovementType.In, request, cancellationToken);

    public Task<StockMovementResponse> StockOutAsync(string productId, StockChangeRequest request, CancellationToken cancellationToken = default)
        => ApplyAsync(productId, StockMovementType.Out, request, cancellationToken);

    public async Task<PagedResult<StockMovementResponse>> GetMovementsAsync(string productId, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        if (await _products.GetByIdAsync(productId, cancellationToken) is null)
            throw new NotFoundException($"'{productId}' Id'li ürün bulunamadı.");

        var page = Math.Max(pagination.Page, 1);
        var pageSize = Math.Clamp(pagination.PageSize, 1, MaxPageSize);

        var (items, totalCount) = await _movements.GetByProductAsync(productId, page, pageSize, cancellationToken);

        return new PagedResult<StockMovementResponse>
        {
            Items = items.Select(StockMovementResponse.FromEntity).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Giriş ve çıkışın ortak işi: kuralları kontrol et, fişi hazırla, elemana uygulat, sonucu yorumla.
    private async Task<StockMovementResponse> ApplyAsync(string productId, StockMovementType type, StockChangeRequest request, CancellationToken cancellationToken)
    {
        // Kural: miktar 1 ile 1.000.000 arasında olmalı.
        if (request.Quantity <= 0)
            throw new BadRequestException("Miktar 0'dan büyük olmalı.");
        if (request.Quantity > MaxQuantity)
            throw new BadRequestException($"Tek işlemde en fazla {MaxQuantity:N0} adet girilebilir.");

        // Kural: her fişin bir nedeni olmalı.
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BadRequestException("Neden (reason) boş olamaz.");

        var movement = new StockMovement
        {
            ProductId = productId,
            Type = type,
            Quantity = request.Quantity,
            Reason = request.Reason.Trim(),
            PerformedBy = DefaultPerformedBy,
            CreatedAt = DateTime.UtcNow
        };

        var product = await _movements.ApplyAsync(movement, cancellationToken);

        if (product is null)
        {
            // Eleman "olmadı" dedi: ya ürün yok ya da (çıkışta) stok yetersiz. Hangisi olduğuna bakıp doğru alarmı çalıyoruz.
            var existing = await _products.GetByIdAsync(productId, cancellationToken)
                ?? throw new NotFoundException($"'{productId}' Id'li ürün bulunamadı.");

            throw new ConflictException(
                $"Yetersiz stok. Mevcut: {existing.StockQuantity}, istenen: {request.Quantity}.");
        }

        return StockMovementResponse.FromEntity(movement);
    }
}
