using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;

namespace ProductStock.Infrastructure.Security;

// Giriş "bilekliği" (JWT) üreten ve gizli anahtarla mühürleyen sınıf.
public class JwtTokenService : ITokenService
{
    // Token içindeki claim (beyan) adları. Kısa ve standart adlar kullanıyoruz;
    // Api tarafı da token'ı okurken aynı adları kullanacak.
    public const string UserIdClaim = JwtRegisteredClaimNames.Sub;          // "sub"
    public const string UsernameClaim = JwtRegisteredClaimNames.UniqueName; // "unique_name"
    public const string FullNameClaim = "fullName";
    public const string RoleClaim = "role";
    public const string SecurityStampClaim = "sstamp";   // güvenlik damgasının bileklikteki kopyası

    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        // Mühür: gizli anahtar + HMAC-SHA256 algoritması.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,

            // Bilekliğin üzerine yazılanlar (claim = beyan). DİKKAT: token şifreli DEĞİL, sadece mühürlü.
            // Herkes içini okuyabilir ama kimse değiştiremez → içine ASLA şifre gibi gizli bilgi konmaz.
            Subject = new ClaimsIdentity(
            [
                new Claim(UserIdClaim, user.Id),
                new Claim(UsernameClaim, user.Username),
                new Claim(FullNameClaim, user.FullName),
                new Claim(RoleClaim, user.Role.ToString()),
                // Damga gizli bilgi değil (şifre değil); sadece "bu bileklik hangi damga döneminde verildi" bilgisi.
                // Mühür sayesinde kimse değiştiremez.
                new Claim(SecurityStampClaim, user.SecurityStamp)
            ]),

            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}
