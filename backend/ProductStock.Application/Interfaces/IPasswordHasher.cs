namespace ProductStock.Application.Interfaces;

// Şifre "kıyma makinesi" sözleşmesi.
// Hash tek yönlüdür: şifreden hash üretmek kolay, hash'ten şifreyi geri elde etmek imkânsız.
public interface IPasswordHasher
{
    // Şifreyi kıyar (içine rastgele "tuz" ekleyerek). Veritabanına bu sonuç yazılır.
    string Hash(string password);

    // Girişte yazılan şifreyi kayıtlı hash ile karşılaştırır.
    bool Verify(string passwordHash, string password);
}
