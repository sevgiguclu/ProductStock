using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Infrastructure;

// Infrastructure katmanının DI kayıtlarını tek yerde toplar.
// Program.cs sadece builder.Services.AddInfrastructure(builder.Configuration) çağırır;
// MongoDB'nin nasıl kurulduğunu bilmek zorunda kalmaz.
public static class DependencyInjection
{
    // "this IServiceCollection" → extension method: IServiceCollection'a dışarıdan metot ekler.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // "MongoDbSettings" bölümünü MongoDbSettings sınıfına bağlar (Options pattern).
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        // Singleton: uygulama boyunca tek MongoDbContext (dolayısıyla tek MongoClient).
        services.AddSingleton<MongoDbContext>();

        return services;
    }
}
