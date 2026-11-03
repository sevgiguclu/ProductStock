namespace ProductStock.Application.Exceptions;

// "İsteğin kendisi hatalı" alarmı (ör. miktar 0 ya da negatif). Api katmanında 400 (Bad Request) cevabına çevrilir.
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
