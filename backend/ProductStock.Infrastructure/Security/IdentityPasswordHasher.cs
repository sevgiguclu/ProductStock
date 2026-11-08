using Microsoft.AspNetCore.Identity;
using ProductStock.Application.Interfaces;

namespace ProductStock.Infrastructure.Security;

// Microsoft'un yıllardır kullanılan, test edilmiş şifre kıyma makinesini (PasswordHasher)
// bizim IPasswordHasher sözleşmemize bağlayan "adaptör".
// Güvenlik kodunu kendimiz yazmıyoruz: kilit yapmayı bilmeden kapıya kilit takmak gibi olurdu.
// İçeride PBKDF2 algoritması + her şifre için rastgele "tuz" kullanılır; aynı şifre bile farklı hash üretir.
public class IdentityPasswordHasher : IPasswordHasher
{
    // PasswordHasher<T> bir kullanıcı tipi ister ama hesaplamada kullanmaz; bu yüzden "object" veriyoruz.
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object NoUser = new();

    public string Hash(string password) => _hasher.HashPassword(NoUser, password);

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(NoUser, passwordHash, password);

        // SuccessRehashNeeded: şifre doğru ama eski bir algoritmayla kıyılmış; yine de "doğru" sayıyoruz.
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
