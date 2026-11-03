using ProductStock.Application.DTOs.Products;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;

namespace ProductStock.Application.Services;

// Ürün işlerinin "müdürü": iş kurallarını uygular, veritabanı işini repository'ye (elemana) bırakır.
// Dikkat: burada MongoDB ile ilgili tek satır yok. Müdür sadece IProductRepository ilanını tanır.
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    // Constructor: DI'dan IProductRepository istiyoruz; DI bize ProductRepository (MongoDB'li hali) verecek.
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        // Select: listedeki her ürünü ProductResponse'a çevir.
        return products.Select(ProductResponse.FromEntity).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var product = await GetExistingProductAsync(id, cancellationToken);
        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var sku = NormalizeSku(request.Sku);

        // Kural: aynı SKU ile iki ürün olamaz.
        if (await _repository.SkuExistsAsync(sku, cancellationToken))
            throw new ConflictException($"'{sku}' stok kodlu bir ürün zaten var.");

        var product = new Product
        {
            Name = request.Name.Trim(),
            Sku = sku,
            Description = request.Description?.Trim(),
            Category = request.Category.Trim(),
            Price = request.Price,
            // Kural: ürün 0 stokla oluşur; stok sadece stok girişi işlemiyle artar.
            StockQuantity = 0,
            CriticalStockLevel = request.CriticalStockLevel,
            // Kural: tarihi kullanıcı değil sunucu yazar, her zaman UTC.
            CreatedAt = DateTime.UtcNow
        };

        // Kayıttan sonra product.Id, MongoDB'nin ürettiği kimlikle dolmuş olur.
        await _repository.CreateAsync(product, cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> UpdateAsync(string id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await GetExistingProductAsync(id, cancellationToken);

        var newSku = NormalizeSku(request.Sku);

        // SKU değiştiyse, yeni SKU başka bir üründe kullanılıyor mu? (Kendi SKU'su ise sorun yok.)
        if (newSku != product.Sku && await _repository.SkuExistsAsync(newSku, cancellationToken))
            throw new ConflictException($"'{newSku}' stok kodlu başka bir ürün zaten var.");

        product.Name = request.Name.Trim();
        product.Sku = newSku;
        product.Description = request.Description?.Trim();
        product.Category = request.Category.Trim();
        product.Price = request.Price;
        product.CriticalStockLevel = request.CriticalStockLevel;
        product.UpdatedAt = DateTime.UtcNow;

        // Bu arada başka biri ürünü silmiş olabilir; o durumda güncelleme hiçbir belgeyi bulamaz.
        var updated = await _repository.UpdateAsync(product, cancellationToken);
        if (!updated)
            throw new NotFoundException($"'{id}' Id'li ürün bulunamadı.");

        return ProductResponse.FromEntity(product);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new NotFoundException($"'{id}' Id'li ürün bulunamadı.");
    }

    // Ürünü getirir; yoksa NotFoundException fırlatır. Birden fazla metotta lazım olduğu için ayrı yazdık.
    // private: sadece bu sınıfın içinden çağrılabilir.
    private async Task<Product> GetExistingProductAsync(string id, CancellationToken cancellationToken)
    {
        // "??" → soldaki null ise sağdakini yap. Ürün null gelirse alarm çalıyoruz.
        return await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"'{id}' Id'li ürün bulunamadı.");
    }

    // SKU'yu tek tip hale getirir: " klv-001 " → "KLV-001".
    // Böylece aynı kod farklı yazıldığında iki ayrı ürün sanılmaz.
    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();
}
