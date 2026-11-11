using ProductStock.Domain.Entities;

namespace ProductStock.Application.Interfaces;

// Kullanıcılar için eleman sözleşmesi.
public interface IUserRepository
{
    // Kullanıcı adına göre bulur (kullanıcı adı küçük harfle verilmeli); yoksa null.
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    // Id'ye göre bulur; yoksa (veya Id geçersizse) null.
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    // Tüm kullanıcılar, kullanıcı adına göre A-Z. (Kullanıcı sayısı az olacağı için sayfalama yok.)
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    // Hiç kullanıcı var mı? (İlk Admin'i otomatik oluşturmak için.)
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    // Yeni kullanıcı kaydeder. Aynı kullanıcı adı varsa ConflictException fırlatır.
    Task CreateAsync(User user, CancellationToken cancellationToken = default);

    // IsActive'i ve güvenlik damgasını birlikte değiştirir (eski bileklikler geçersiz olur). Kullanıcı yoksa false.
    Task<bool> SetActiveAsync(string id, bool isActive, string securityStamp, CancellationToken cancellationToken = default);

    // Şifre kıymasını ve güvenlik damgasını birlikte değiştirir (eski bileklikler geçersiz olur). Kullanıcı yoksa false.
    Task<bool> UpdatePasswordHashAsync(string id, string passwordHash, string securityStamp, CancellationToken cancellationToken = default);

    // Sadece güvenlik damgasını yazar (damgası olmayan eski kullanıcılar için ilk girişte). Kullanıcı yoksa false.
    Task<bool> SetSecurityStampAsync(string id, string securityStamp, CancellationToken cancellationToken = default);
}
