using ProductStock.Domain.Entities;

namespace ProductStock.Application.Interfaces;

// Kullanıcılar için eleman sözleşmesi.
public interface IUserRepository
{
    // Kullanıcı adına göre bulur (kullanıcı adı küçük harfle verilmeli); yoksa null.
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    // Hiç kullanıcı var mı? (İlk Admin'i otomatik oluşturmak için.)
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    // Yeni kullanıcı kaydeder. Aynı kullanıcı adı varsa ConflictException fırlatır.
    Task CreateAsync(User user, CancellationToken cancellationToken = default);
}
