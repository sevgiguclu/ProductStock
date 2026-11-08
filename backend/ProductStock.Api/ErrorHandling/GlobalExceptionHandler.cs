using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductStock.Application.Exceptions;

namespace ProductStock.Api.ErrorHandling;

// Merkezi hata yakalayıcı: uygulamanın herhangi bir yerinde fırlatılan exception'ı (alarmı) yakalar
// ve kullanıcıya uygun HTTP kodu + standart hata gövdesi (ProblemDetails) döner.
// Böylece controller'lara tek tek try/catch yazmak zorunda kalmayız.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // switch: exception'ın türüne göre hangi kod ve başlığı döneceğimize karar veriyoruz.
        var (statusCode, title, detail) = exception switch
        {
            BadRequestException => (StatusCodes.Status400BadRequest, "Geçersiz istek", exception.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Kimlik doğrulanamadı", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Kayıt bulunamadı", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Çakışma", exception.Message),
            // Beklenmeyen hata: ayrıntıyı kullanıcıya göstermiyoruz (iç bilgi sızdırabilir), sadece log'a yazıyoruz.
            _ => (StatusCodes.Status500InternalServerError, "Sunucu hatası", "Beklenmeyen bir hata oluştu.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Beklenmeyen hata: {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        // true: "bu hatayı ben hallettim", başka bir yakalayıcıya gerek yok.
        return true;
    }
}
