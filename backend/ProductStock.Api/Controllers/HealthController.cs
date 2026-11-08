using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Api.Controllers;

// [ApiController]: model doğrulama, otomatik 400 yanıtı gibi API davranışlarını açar.
// [Route]: bu controller'ın adresi → "api/health" ([controller] = sınıf adından "Controller" atılmış hali).
// [AllowAnonymous]: giriş yapmadan erişilebilir. Sağlık kontrolünü izleme araçları (ileride Docker) çağıracak,
// onların bilekliği yok. İçinde gizli bilgi yok, sadece "bağlı / bağlanılamadı" der.
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly MongoDbContext _context;

    // MongoDbContext'i DI'dan constructor üzerinden alıyoruz (constructor injection).
    public HealthController(MongoDbContext context)
    {
        _context = context;
    }

    // GET api/health/db → MongoDB'ye ping atar, sonucu döner.
    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase(CancellationToken cancellationToken)
    {
        var isConnected = await _context.PingAsync(cancellationToken);

        return isConnected
            ? Ok(new { database = "MongoDB", status = "bağlı" })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new { database = "MongoDB", status = "bağlanılamadı" });
    }
}
