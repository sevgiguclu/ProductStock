using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductStock.Domain.Enums;
using ProductStock.Infrastructure.Security;

namespace ProductStock.Api.Security;

// "Kapı görevlisi" ayarları: gelen isteklerdeki bilekliği (JWT) kim, nasıl kontrol edecek?
public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Authentication (kimlik doğrulama) = "Sen kimsin?"
        // Varsayılan yöntem JWT Bearer: istek başlığında "Authorization: Bearer <token>" arar.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Görevlinin kontrol listesi. Ayarları doğrudan appsettings'ten okumak yerine
        // token'ı ÜRETEN taraftaki JwtSettings'i (açılışta doğrulanan) kullanıyoruz:
        // üreten ve kontrol eden aynı anahtarı, aynı adları kullansın diye tek kaynak.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                // false: claim adlarını olduğu gibi bırak ("role" → "role").
                // true olsaydı .NET bunları uzun eski adlara çevirirdi
                // ("role" → "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Mühür gerçek mi? Bizim gizli anahtarımızla mı basılmış?
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    // Sadece bizim kullandığımız algoritmayı kabul et (başka algoritmayla gelen sahte token'lara kapı kapalı).
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

                    // Bilekliği BİZ mi verdik? (iss)
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    // Bileklik BİZİM uygulamamız için mi verilmiş? (aud)
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    // Süresi dolmuş mu? (exp, nbf)
                    ValidateLifetime = true,
                    // Saat farkı toleransı. Varsayılan 5 dakika; yani süresi dolan token 5 dk daha geçerdi.
                    // 30 saniyeye indiriyoruz: sunucu saatleri arasındaki küçük farklara yeter.
                    ClockSkew = TimeSpan.FromSeconds(30),

                    // Hangi claim "kullanıcı adı", hangisi "rol"? Token'ı üretirken kullandığımız adların aynısı.
                    // Böylece User.Identity.Name → "admin", User.IsInRole("Admin") → doğru çalışır.
                    NameClaimType = JwtTokenService.UsernameClaim,
                    RoleClaimType = JwtTokenService.RoleClaim
                };
            });

        // Authorization (yetkilendirme) = "Buraya girebilir misin?"
        // Fallback (yedek) kural: hiçbir kural yazılmamış her adres için "giriş yapmış olmak ŞART".
        // "Varsayılan olarak kilitli": yeni bir controller eklerken [Authorize] yazmayı unutsak bile kapı kapalı kalır.
        // Açık kalması gereken adresler (login, health) [AllowAnonymous] ile bilerek açılır.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            // "AdminOnly" kuralı: bileklikteki "role" claim'i "Admin" olmalı.
            // nameof(UserRole.Admin) → "Admin". Elle "Admin" yazmak yerine enum'dan alıyoruz:
            // enum'daki ad değişirse burası derlenmez ve hatayı hemen görürüz (yazım hatası riski yok).
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(nameof(UserRole.Admin)));

        return services;
    }
}
