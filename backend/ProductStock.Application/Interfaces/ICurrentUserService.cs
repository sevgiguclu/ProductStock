namespace ProductStock.Application.Interfaces;

// "Şu an işlemi yapan kullanıcı kim?" sorusunun sözleşmesi.
// Application katmanı HTTP'yi (istekleri, bileklikleri) TANIMAZ; sadece bu soruyu sorar.
// Cevabı Api katmanı verir (bileklikten okuyarak). Birim testlerinde sahte bir cevap verilebilir.
public interface ICurrentUserService
{
    // Giriş yapan kullanıcının adı (ör. "depo1"). Giriş yapılmamışsa null.
    string? Username { get; }
}
