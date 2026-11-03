using ProductStock.Api.ErrorHandling;
using ProductStock.Application;
using ProductStock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1) SERVİSLER: DI "malzeme deposunu" dolduruyoruz.
// ---------------------------------------------------------------

// Controller sınıflarını (HealthController, ProductsController...) etkinleştirir.
builder.Services.AddControllers();

// Application katmanı (ProductService gibi iş kuralları)
builder.Services.AddApplication();

// Infrastructure katmanı (MongoDB bağlantısı, repository'ler)
builder.Services.AddInfrastructure(builder.Configuration);

// Merkezi hata yakalayıcı + standart hata gövdesi (ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// API'nin tarifini (OpenAPI belgesi) üretir. Swagger düzeni 7. günde yapılacak.
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------------------------------------------------------------
// 2) PIPELINE: Gelen her istek bu "istasyonlardan" (middleware) sırayla geçer.
//    Sıra önemlidir.
// ---------------------------------------------------------------

// En başta: arkadaki herhangi bir istasyonda hata olursa GlobalExceptionHandler yakalar.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Sadece geliştirmede: API tarifini /openapi/v1.json adresinde yayınlar.
    app.MapOpenApi();
}

// http ile gelen isteği https'e yönlendirir.
app.UseHttpsRedirection();

// Yetki kontrolü (JWT ve roller 5-6. günde eklenecek).
app.UseAuthorization();

// İsteği adresine göre doğru controller metoduna gönderir.
app.MapControllers();

app.Run();
