using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Application.DTOs.Stock;

// Stok fişinin kullanıcıya giden hali.
public class StockMovementResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    // JSON'da "In" / "Out" olarak görünür (Program.cs'teki JsonStringEnumConverter sayesinde).
    public StockMovementType Type { get; set; }

    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static StockMovementResponse FromEntity(StockMovement movement) => new()
    {
        Id = movement.Id,
        ProductId = movement.ProductId,
        ProductSku = movement.ProductSku,
        ProductName = movement.ProductName,
        Type = movement.Type,
        Quantity = movement.Quantity,
        StockAfter = movement.StockAfter,
        Reason = movement.Reason,
        PerformedBy = movement.PerformedBy,
        CreatedAt = movement.CreatedAt
    };
}
