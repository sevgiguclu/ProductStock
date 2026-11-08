using Microsoft.Extensions.DependencyInjection;
using ProductStock.Application.Interfaces;
using ProductStock.Application.Services;

namespace ProductStock.Application;

// Application katmanının DI kayıtları. Program.cs: builder.Services.AddApplication();
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Biri IProductService (müdür sözleşmesi) isterse ona ProductService ver. Her istek için yeni bir tane.
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
