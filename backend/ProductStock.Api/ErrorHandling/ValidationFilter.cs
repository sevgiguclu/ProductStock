using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProductStock.Api.ErrorHandling;

// "Kontrol noktası" (action filter): controller metodu çalışmadan HEMEN ÖNCE devreye girer.
// Metoda gelen her form (ör. LoginRequest) için yazılmış bir kural sınıfı (IValidator<LoginRequest>) varsa çalıştırır.
// Hata varsa metodu HİÇ çalıştırmadan 400 + alan alan hata listesi döner.
// Böylece her controller'a / servise "önce doğrula" satırı yazmayı unutma riski olmaz.
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Hata listesi: alan adı → mesajlar. Ör. "username" → ["Kullanıcı adı boş olamaz."]
        var errors = new Dictionary<string, List<string>>();

        // ActionArguments: metoda gelen parametreler (id, request, ...).
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            // "Bu formun türü için bir kural sınıfı kayıtlı mı?" → IValidator<FormTürü>'nü DI'dan iste.
            // MakeGenericType: tür adını çalışırken oluşturur (LoginRequest → IValidator<LoginRequest>).
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;   // kural yazılmamış (ör. string id) → geç

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            foreach (var error in result.Errors)
                Add(errors, ToCamelCase(error.PropertyName), error.ErrorMessage);
        }

        if (errors.Count > 0)
        {
            // Result'ı doldurmak = "metodu çalıştırma, cevap bu".
            context.Result = CreateValidationProblem(context.HttpContext, errors);
            return;
        }

        await next();   // her şey yolunda → asıl controller metoduna devam
    }

    // ASP.NET'in KENDİ yakaladığı hatalar (JSON okunamadı, "Patron" gibi tanımsız rol, boş gövde) için.
    // Bu mesajlar İngilizce ve bazen iç yapımızı sızdırıyor ("ProductStock.Domain.Enums.UserRole" gibi).
    // Hepsini Türkçe ve genel mesajlara çeviriyoruz; ayrıntı dışarı verilmez.
    public static BadRequestObjectResult CreateFromModelState(HttpContext httpContext, ModelStateDictionary modelState)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var (key, entry) in modelState)
        {
            if (entry.Errors.Count == 0)
                continue;

            if (key == "$")
                Add(errors, "body", "Gönderilen JSON okunamadı (biçim bozuk).");
            else if (key.StartsWith("$."))
                // "$.role" → "role": değer bu alanın türüne çevrilemedi (ör. rol "Patron", sayı yerine metin).
                Add(errors, ToCamelCase(key[2..]), "Bu alanın değeri geçersiz.");
            else if (key is "" or "request")
                // Gövde hiç yok veya okunamadı ("": ASP.NET'in boş gövde anahtarı, "request": parametre adlarımız).
                Add(errors, "body", "İstek gövdesi boş veya okunamadı.");
            else
                foreach (var error in entry.Errors)
                    Add(errors, ToCamelCase(key), error.ErrorMessage);
        }

        // JSON'da bir alan bozuksa ASP.NET ayrıca "gövde boş" da der; asıl sebep zaten listede olduğu için onu çıkar.
        if (errors.Count > 1)
            errors.Remove("body");

        return CreateValidationProblem(httpContext, errors);
    }

    // Ortak cevap biçimi (ValidationProblemDetails): title, status, detail + "errors" listesi.
    private static BadRequestObjectResult CreateValidationProblem(HttpContext httpContext, Dictionary<string, List<string>> errors)
    {
        var problem = new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Doğrulama hatası",
            Detail = "Gönderilen bilgilerde hata var. Ayrıntılar 'errors' alanında.",
            Instance = httpContext.Request.Path
        };
        return new BadRequestObjectResult(problem);
    }

    private static void Add(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var list))
            errors[field] = list = [];
        if (!list.Contains(message))
            list.Add(message);
    }

    // "Username" → "username" (JSON'daki yazımla aynı olsun).
    private static string ToCamelCase(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}
