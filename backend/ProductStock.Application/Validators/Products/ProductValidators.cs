using FluentValidation;
using ProductStock.Application.DTOs.Products;

namespace ProductStock.Application.Validators.Products;

// Ürün formlarının ORTAK kuralları (ekleme ve düzenleme aynı alanlara sahip).
// internal: sadece bu proje içinden görünür. Otomatik kayıt (AddValidatorsFromAssembly) internal sınıfları ALMAZ;
// zaten tek başına kullanılmayacak, aşağıdaki iki sınıf onu "Include" ile içeri alıyor.
internal class ProductFieldsValidator : AbstractValidator<IProductFields>
{
    // SKU kalıbı: büyük harf (A-Z), rakam, tire; 2-50 karakter. Ör. "KLV-001".
    // Kontrol, servisin yapacağı düzeltmeden (boşluk sil + büyük harf) SONRAKİ hali üzerinde yapılır:
    // " klv-001 " yazan kullanıcı reddedilmez, çünkü kaydedilecek hali "KLV-001".
    private const string SkuPattern = "^[A-Z0-9-]{2,50}$";

    public ProductFieldsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        // RuleFor içine alanın kendisi yerine "düzeltilmiş halini" veriyoruz.
        // OverridePropertyName: hata listesinde alan adı "sku" görünsün (ifadenin kendisi değil).
        RuleFor(x => (x.Sku ?? string.Empty).Trim().ToUpperInvariant())
            .Matches(SkuPattern).WithMessage("Stok kodu (SKU) 2-50 karakter olmalı ve sadece harf (A-Z), rakam ve tire içermeli.")
            .OverridePropertyName("sku");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");   // boş olabilir (isteğe bağlı)

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori boş olamaz.")
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.");

        RuleFor(x => x.Price)
            // 0 TL'lik ürün bir stok sisteminde büyük ihtimalle yazım hatasıdır.
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalı.")
            .LessThanOrEqualTo(10_000_000).WithMessage("Fiyat en fazla 10.000.000 olabilir.")
            // PrecisionScale(12, 2, true): toplam en fazla 12 basamak, virgülden sonra en fazla 2 (kuruş).
            // 749,999 TL gibi kuruşun altına inen fiyatları reddeder. true: "749,90" ile "749,9" aynı sayılsın.
            .PrecisionScale(12, 2, true).WithMessage("Fiyat virgülden sonra en fazla 2 basamak (kuruş) içerebilir.");

        RuleFor(x => x.CriticalStockLevel)
            .InclusiveBetween(0, 1_000_000).WithMessage("Kritik stok seviyesi 0 ile 1.000.000 arasında olmalı.");
    }
}

// Ekleme formu: ortak kuralların hepsi geçerli. (İleride sadece eklemeye özel kural gerekirse buraya yazılır.)
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        // Include: başka bir kural sınıfının bütün kurallarını içeri al.
        Include(new ProductFieldsValidator());
    }
}

// Düzenleme formu: aynı ortak kurallar.
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        Include(new ProductFieldsValidator());
    }
}
