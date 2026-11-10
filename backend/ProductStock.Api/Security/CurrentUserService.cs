using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Security;

// ICurrentUserService'in gerçek hali: kullanıcı adını o anki isteğin bilekliğinden okur.
// IHttpContextAccessor: "şu an işlenen HTTP isteğine" her yerden ulaşmayı sağlayan yardımcı.
// (Controller'da istek zaten elimizde; ama burası controller değil, o yüzden bu yardımcıya ihtiyaç var.)
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // HttpContext?.User → isteğin üzerindeki kullanıcı (kapı görevlisi bilekliği okuyup buraya yazdı).
    // Identity?.Name → görevliye "kullanıcı adı = unique_name claim'i" dedik (NameClaimType), o yüzden "depo1" gelir.
    // "?." (null-conditional): soldaki null ise hata verme, sonucu null yap.
    public string? Username => _httpContextAccessor.HttpContext?.User.Identity?.Name;
}
