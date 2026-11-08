using ProductStock.Domain.Entities;

namespace ProductStock.Application.Interfaces;

// Giriş "bilekliği" (JWT) üretme sözleşmesi.
public interface ITokenService
{
    // Kullanıcı için mühürlü bir token üretir; token metnini ve son kullanma zamanını (UTC) döner.
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
