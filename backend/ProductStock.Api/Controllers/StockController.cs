using Microsoft.AspNetCore.Mvc;
using ProductStock.Application.DTOs.Common;
using ProductStock.Application.DTOs.Stock;
using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Controllers;

// Stok işlemlerini karşılayan görevli. Adres ürünün altında: "şu ürünün stoğu üzerinde işlem".
//   POST api/products/{productId}/stock/in         → giriş
//   POST api/products/{productId}/stock/out        → çıkış
//   GET  api/products/{productId}/stock/movements  → fiş geçmişi
[ApiController]
[Route("api/products/{productId}/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    // Stok girişi. Gövde: { "quantity": 50, "reason": "Tedarikçiden alım" }
    [HttpPost("in")]
    public async Task<ActionResult<StockMovementResponse>> StockIn(string productId, StockChangeRequest request, CancellationToken cancellationToken)
    {
        var movement = await _stockService.StockInAsync(productId, request, cancellationToken);
        return Ok(movement);
    }

    // Stok çıkışı. Stok yetersizse 409 döner (GlobalExceptionHandler).
    [HttpPost("out")]
    public async Task<ActionResult<StockMovementResponse>> StockOut(string productId, StockChangeRequest request, CancellationToken cancellationToken)
    {
        var movement = await _stockService.StockOutAsync(productId, request, cancellationToken);
        return Ok(movement);
    }

    // Ürünün stok fişleri, en yeni önce: ?page=1&pageSize=20
    [HttpGet("movements")]
    public async Task<ActionResult<PagedResult<StockMovementResponse>>> GetMovements(string productId, [FromQuery] PaginationParameters pagination, CancellationToken cancellationToken)
    {
        var result = await _stockService.GetMovementsAsync(productId, pagination, cancellationToken);
        return Ok(result);
    }
}
