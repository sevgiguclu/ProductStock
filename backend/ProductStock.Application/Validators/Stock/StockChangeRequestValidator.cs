using FluentValidation;
using ProductStock.Application.DTOs.Stock;

namespace ProductStock.Application.Validators.Stock;

// Stok giriş/çıkış formu: { "quantity": 10, "reason": "Tedarikçiden alım" }
// Not: "Stok YETERLİ mi?" burada kontrol edilemez (ürünün o anki stoğuna bakmak gerekir) → serviste/veritabanında kalır.
public class StockChangeRequestValidator : AbstractValidator<StockChangeRequest>
{
    // Tek fişte izin verilen en büyük miktar. int'in bir tavanı var (~2,1 milyar);
    // dev bir sayı stoğu taşırıp anlamsız bir değere çevirmesin.
    public const int MaxQuantity = 1_000_000;

    public StockChangeRequestValidator()
    {
        RuleFor(x => x.Quantity)
            // Miktar her zaman pozitif yazılır; giriş mi çıkış mı olduğunu adres (in/out) söyler.
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalı.")
            .LessThanOrEqualTo(MaxQuantity).WithMessage($"Tek işlemde en fazla {MaxQuantity:N0} adet girilebilir.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Neden (reason) boş olamaz.")   // her fişin bir nedeni olmalı
            .MaximumLength(500).WithMessage("Neden en fazla 500 karakter olabilir.");
    }
}
