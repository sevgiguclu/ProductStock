using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductStock.Application.Interfaces;
using ProductStock.Infrastructure.Persistence;
using ProductStock.Infrastructure.Repositories;
using ProductStock.Infrastructure.Security;

namespace ProductStock.Infrastructure;

// Infrastructure katmanının DI kayıtlarını tek yerde toplar.
// Program.cs sadece builder.Services.AddInfrastructure(builder.Configuration) çağırır;
// MongoDB'nin nasıl kurulduğunu bilmek zorunda kalmaz.
public static class DependencyInjection
{
    // "this IServiceCollection" → extension method: IServiceCollection'a dışarıdan metot ekler.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // C# ↔ MongoDB tercüman kurallarını yükle. Veritabanına ilk erişimden ÖNCE çalışmalı.
        MongoMappings.Register();

        // "MongoDbSettings" bölümünü MongoDbSettings sınıfına bağlar (Options pattern).
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        // Singleton: uygulama boyunca tek MongoDbContext (dolayısıyla tek MongoClient).
        services.AddSingleton<MongoDbContext>();

        // Biri IProductRepository (sözleşme) isterse ona ProductRepository (MongoDB'li hali) ver.
        // Scoped: her HTTP isteği için yeni bir ProductRepository; istek bitince o da biter.
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // --- Güvenlik ---

        // JWT ayarları + açılışta kontrol: mühür anahtarı en az 32 bayt değilse uygulama hiç başlamasın.
        // ValidateOnStart: hatayı ilk girişte değil, uygulama açılırken yakala ("erken ve yüksek sesle başarısız ol").
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => Encoding.UTF8.GetByteCount(s.SecretKey) >= 32,
                "JwtSettings:SecretKey en az 32 karakter olmalı (User Secrets ile ayarlayın).")
            .Validate(s => s.ExpiryMinutes > 0, "JwtSettings:ExpiryMinutes 0'dan büyük olmalı.")
            .ValidateOnStart();

        services.Configure<SeedAdminSettings>(configuration.GetSection(SeedAdminSettings.SectionName));

        // Kıyma makinesi ve bileklik üreticisi durumsuz (state tutmuyor) → uygulama boyunca tek nesne yeterli.
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Uygulama açılırken çalışan işler (hosted service). Kayıt SIRASIYLA çalışırlar:
        // önce index'ler kurulur, sonra (gerekirse) ilk Admin oluşturulur.
        services.AddHostedService<MongoIndexInitializer>();
        services.AddHostedService<AdminSeeder>();

        return services;
    }
}
