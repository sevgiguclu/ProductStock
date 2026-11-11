using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Domain.Enums;

namespace ProductStock.Infrastructure.Security;

// Uygulama açılırken çalışır: veritabanında HİÇ kullanıcı yoksa ilk Admin'i oluşturur.
// "Tavuk-yumurta" sorununu çözer: kullanıcıları Admin oluşturacak, peki ilk Admin'i kim oluşturacak?
// Zaten kullanıcı varsa hiçbir şey yapmaz.
public class AdminSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedAdminSettings _settings;
    private readonly ILogger<AdminSeeder> _logger;

    // IServiceScopeFactory: Hosted service uygulama boyunca tek (singleton) yaşar;
    // IUserRepository ise her istek için yeni (scoped). Singleton, scoped bir servisi doğrudan alamaz;
    // bu yüzden kısa süreli bir "kapsam" (scope) açıp repository'yi onun içinden alıyoruz.
    public AdminSeeder(IServiceScopeFactory scopeFactory, IPasswordHasher passwordHasher,
        IOptions<SeedAdminSettings> options, ILogger<AdminSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _passwordHasher = passwordHasher;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        if (await users.AnyAsync(cancellationToken))
            return;

        if (string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning("Hiç kullanıcı yok ve SeedAdmin:Password ayarlanmamış; ilk Admin oluşturulmadı.");
            return;
        }

        var admin = new User
        {
            Username = _settings.Username.Trim().ToLowerInvariant(),
            FullName = _settings.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(_settings.Password),
            Role = UserRole.Admin,
            IsActive = true,
            SecurityStamp = User.NewSecurityStamp(),
            CreatedAt = DateTime.UtcNow
        };

        await users.CreateAsync(admin, cancellationToken);
        _logger.LogInformation("İlk Admin kullanıcısı oluşturuldu: {Username}", admin.Username);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
