using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProductStock.Application.Interfaces;
using ProductStock.Infrastructure.Security;

namespace ProductStock.Api.Security;

// Kapı görevlisinin EK kontrolü: mühür ve süre tamam olsa bile bileklik hâlâ geçerli mi?
// JwtBearer, bilekliği doğruladıktan HEMEN SONRA bu metodu çağırır (OnTokenValidated olayı).
//
// Neden gerekli? Bileklik (JWT) kendi kendine yeten bir belge; görevli normalde veritabanına SORMAZ.
// O yüzden pasif yapılan ya da şifresini değiştiren kullanıcının eski bilekliği süresi dolana kadar çalışırdı.
// Burada veritabanına bakıp "kullanıcı hâlâ aktif mi, bileklikteki damga güncel mi?" diye soruyoruz.
// Bedeli: korunan her istekte veritabanına 1 küçük sorgu (Id ile, index'li → çok hızlı).
public static class SecurityStampValidator
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var userId = principal?.FindFirst(JwtTokenService.UserIdClaim)?.Value;
        var tokenStamp = principal?.FindFirst(JwtTokenService.SecurityStampClaim)?.Value;

        // Damgası olmayan bileklik: bu özellik gelmeden önce verilmiş. Kabul etmiyoruz → yeniden giriş gerekir.
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenStamp))
        {
            context.Fail("Bileklikte güvenlik damgası yok.");
            return;
        }

        // RequestServices: bu isteğe ait DI kapsamı (scoped repository'yi buradan alabiliriz).
        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await users.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        // Kullanıcı silinmiş / pasif / damgası değişmiş (şifre değişti veya pasif yapıldı) → bileklik geçersiz.
        // Fail: görevli isteği 401 ile reddeder (controller'a hiç ulaşmaz).
        if (user is null || !user.IsActive || !string.Equals(user.SecurityStamp, tokenStamp, StringComparison.Ordinal))
        {
            context.Fail("Bileklik artık geçerli değil.");
        }
    }
}
