using ProductStock.Application.DTOs.Common;
using ProductStock.Application.DTOs.Products;

namespace ProductStock.Application.Interfaces;

// Ürün işlemlerinin "müdür" sözleşmesi. Controller sadece bu sözleşmeyi tanır.
// Repository'den farkı: dışarıyla DTO (form) üzerinden konuşur ve iş kurallarını uygular.
public interface IProductService
{
    // Arama/filtre/sıralama/sayfalama ile ürün listesi. Sayfa değerleri güvenli sınırlara çekilir.
    Task<PagedResult<ProductResponse>> SearchAsync(ProductQueryParameters query, CancellationToken cancellationToken = default);

    // Ürün yoksa NotFoundException fırlatır.
    Task<ProductResponse> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    // Aynı SKU varsa ConflictException fırlatır. Ürün 0 stokla oluşur.
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    // Ürün yoksa NotFoundException, yeni SKU başka bir üründe varsa ConflictException fırlatır.
    Task<ProductResponse> UpdateAsync(string id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    // Ürün yoksa NotFoundException fırlatır.
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
