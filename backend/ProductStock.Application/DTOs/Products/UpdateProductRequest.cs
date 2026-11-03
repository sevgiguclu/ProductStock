namespace ProductStock.Application.DTOs.Products;

// Ürün düzenleme "formu". Hangi ürünün düzenleneceği (Id) adresten gelir: PUT api/products/{id}
// StockQuantity yok: stok sadece stok giriş/çıkış işlemleriyle değişir.
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CriticalStockLevel { get; set; }
}
