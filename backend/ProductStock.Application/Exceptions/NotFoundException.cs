namespace ProductStock.Application.Exceptions;

// "Aranan kayıt bulunamadı" alarmı. Api katmanında 404 (Not Found) cevabına çevrilir.
// ": Exception" → C#'ın hazır Exception sınıfından türetiyoruz (onun tüm özelliklerini alıyor).
public class NotFoundException : Exception
{
    // ": base(message)" → mesajı, türediğimiz Exception sınıfının constructor'ına iletiyoruz.
    public NotFoundException(string message) : base(message)
    {
    }
}
