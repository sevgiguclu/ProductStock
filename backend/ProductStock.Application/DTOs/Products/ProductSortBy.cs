namespace ProductStock.Application.DTOs.Products;

// Ürün listesinin hangi alana göre sıralanacağı. Adreste ?sortBy=price gibi yazılır.
public enum ProductSortBy
{
    Name,
    Price,
    StockQuantity,
    CreatedAt
}
