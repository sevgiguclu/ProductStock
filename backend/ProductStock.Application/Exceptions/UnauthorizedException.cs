namespace ProductStock.Application.Exceptions;

// "Kimliğini doğrulayamadım" alarmı (ör. hatalı kullanıcı adı/şifre). Api katmanında 401 (Unauthorized) cevabına çevrilir.
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
