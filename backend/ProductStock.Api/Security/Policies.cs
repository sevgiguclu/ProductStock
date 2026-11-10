namespace ProductStock.Api.Security;

// Yetki kurallarının (policy) adları. Kuralın İÇERİĞİ JwtAuthenticationExtensions'ta tek yerde tanımlı;
// controller'lar sadece adını kullanır: [Authorize(Policy = Policies.AdminOnly)]
// const: derleme anında sabit değer; attribute'lar (köşeli parantezli etiketler) sadece sabit değer kabul eder.
public static class Policies
{
    // Sadece Admin rolü: ürün ekleme/düzenleme/silme, kullanıcı yönetimi.
    public const string AdminOnly = "AdminOnly";
}
