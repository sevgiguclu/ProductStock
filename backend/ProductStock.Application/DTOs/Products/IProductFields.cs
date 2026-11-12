namespace ProductStock.Application.DTOs.Products;

// Ürün ekleme ve düzenleme formlarının ORTAK alanları.
// İki form da bu sözleşmeyi imzalar; böylece doğrulama kurallarını bir kez (bu sözleşme için) yazarız.
public interface IProductFields
{
    string Name { get; }
    string Sku { get; }
    string? Description { get; }
    string Category { get; }
    decimal Price { get; }
    int CriticalStockLevel { get; }
}
