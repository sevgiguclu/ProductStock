using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductStock.Application.Interfaces;
using ProductStock.Infrastructure.Persistence;
using ProductStock.Infrastructure.Repositories;

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

        // Uygulama açılırken index'leri oluşturan iş (hosted service).
        services.AddHostedService<MongoIndexInitializer>();

        return services;
    }
}
