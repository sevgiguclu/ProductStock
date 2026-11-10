using System.Text.Json.Serialization;
using ProductStock.Api.ErrorHandling;
using ProductStock.Api.Security;
using ProductStock.Application;
using ProductStock.Application.Interfaces;
using ProductStock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1) SERVİSLER: DI "malzeme deposunu" dolduruyoruz.
// ---------------------------------------------------------------

// Controller sınıflarını (HealthController, ProductsController...) etkinleştirir.
// JsonStringEnumConverter: JSON'da enum'lar sayı (0, 1) yerine metin ("In", "Out") olarak yazılsın/okunsun.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Application katmanı (ProductService gibi iş kuralları)
builder.Services.AddApplication();

// Infrastructure katmanı (MongoDB bağlantısı, repository'ler)
builder.Services.AddInfrastructure(builder.Configuration);

// Kimlik doğrulama (JWT bilekliği kontrolü) + "varsayılan olarak kilitli" yetki kuralı
builder.Services.AddJwtAuthentication();

// "Şu anki kullanıcı kim?" → bileklikten okuyan CurrentUserService.
// AddHttpContextAccessor: şu anki HTTP isteğine controller dışından da ulaşmayı sağlar.
// Scoped: her istek için yeni nesne (her isteğin kullanıcısı farklı).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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
    // AllowAnonymous: "varsayılan olarak kilitli" kuralı bu tarif sayfasını da kilitlemesin.
    app.MapOpenApi().AllowAnonymous();
}

// http ile gelen isteği https'e yönlendirir.
app.UseHttpsRedirection();

// 1. görevli – Kimlik doğrulama: "Sen kimsin?" Bilekliği okur, mührünü ve süresini kontrol eder.
// 2. görevli – Yetkilendirme: "Buraya girebilir misin?" Kurallara bakar.
// SIRA ÖNEMLİ: önce kim olduğunu bilmeliyiz, sonra iznine bakabiliriz.
app.UseAuthentication();
app.UseAuthorization();

// İsteği adresine göre doğru controller metoduna gönderir.
app.MapControllers();

app.Run();
