namespace ProductStock.Application.Exceptions;

// "Bu işlem mevcut verilerle çakışıyor" alarmı (ör. aynı SKU ile ikinci ürün).
// Api katmanında 409 (Conflict) cevabına çevrilir.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
