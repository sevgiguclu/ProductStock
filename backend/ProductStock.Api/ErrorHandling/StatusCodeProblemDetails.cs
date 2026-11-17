using Microsoft.AspNetCore.Http;

namespace ProductStock.Api.ErrorHandling;

// GÖVDESİ BOŞ giden hata cevaplarına (kapı görevlisinin 401/403'ü, olmayan adres 404, yanlış metot 405)
// Türkçe başlık ve açıklama ekler. Bu cevapları bizim kodumuz değil ASP.NET'in kendisi üretir;
// GlobalExceptionHandler onları görmez. Program.cs'teki UseStatusCodePages + AddProblemDetails ile çalışır.
public static class StatusCodeProblemDetails
{
    public static void Customize(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;

        // Detail doluysa bu cevabı zaten biri (bizim kod) açıklamalı yazmış → dokunma.
        if (problem.Detail is not null)
            return;

        (problem.Title, problem.Detail) = problem.Status switch
        {
            // 401: kim olduğunu bilmiyoruz (bileklik yok / sahte / süresi dolmuş / damgası eski).
            // Sebebi ayrıntılı söylemiyoruz (saldırgana ipucu vermemek için); ön yüz bunu görünce giriş ekranına yönlendirir.
            StatusCodes.Status401Unauthorized =>
                ("Kimlik doğrulanamadı", "Bu işlem için giriş yapmalısınız. Oturumunuzun süresi dolmuş olabilir."),

            // 403: kim olduğunu biliyoruz ama rolü bu işe yetmiyor.
            StatusCodes.Status403Forbidden =>
                ("Yetkisiz işlem", "Bu işlem için yetkiniz yok."),

            // 404: adres hiç yok (ör. /api/urunler). Kayıt bulunamadı 404'ü bizim koddan açıklamalı gelir, buraya düşmez.
            StatusCodes.Status404NotFound =>
                ("Adres bulunamadı", "İstenen adres bulunamadı."),

            // 405: adres var ama bu metotla değil (ör. sağlık kontrolüne DELETE).
            StatusCodes.Status405MethodNotAllowed =>
                ("İzin verilmeyen metot", "Bu adres bu HTTP metodunu desteklemiyor."),

            // Diğerleri: ASP.NET'in varsayılan başlığı kalsın, genel bir açıklama ekle.
            _ => (problem.Title, "İstek işlenemedi.")
        };
    }
}
