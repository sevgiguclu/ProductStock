using Microsoft.AspNetCore.Mvc;
using ProductStock.Application.DTOs.Products;
using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Controllers;

// Ürün isteklerini karşılayan "danışma masası görevlisi". Adresi: api/products
// Görevli karar vermez: işi IProductService'e (müdüre) iletir, cevabı geri verir.
// Hata durumları (404, 409) burada yazılmaz; müdürün fırlattığı alarmı GlobalExceptionHandler yakalar.
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET api/products → tüm ürünler
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    // GET api/products/{id} → tek ürün. {id} adresten okunur ve "id" parametresine yerleşir.
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    // POST api/products → yeni ürün. Form (CreateProductRequest) isteğin gövdesinden (JSON) okunur.
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(request, cancellationToken);

        // 201 Created: "oluşturuldu" + yeni ürünün adresi (Location başlığında) + ürünün kendisi.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/products/{id} → ürünü güncelle (stok hariç).
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductResponse>> Update(string id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var updated = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    // DELETE api/products/{id} → ürünü sil. 204 No Content: "silindi, gösterecek bir şey yok".
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
