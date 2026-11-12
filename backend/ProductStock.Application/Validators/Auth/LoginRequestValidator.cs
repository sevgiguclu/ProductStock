using FluentValidation;
using ProductStock.Application.DTOs.Auth;

namespace ProductStock.Application.Validators.Auth;

// Giriş formunun kuralları. AbstractValidator<T>: "T formunun kurallarını bu sınıf tanımlar."
// Kurallar constructor'da yazılır; her RuleFor bir alanı seçer, arkasından gelenler o alanın kurallarıdır.
// Not: Burada sadece "form düzgün doldurulmuş mu?" bakılır. "Şifre doğru mu?" iş kuralıdır, AuthService'te kalır.
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")      // NotEmpty: null, "" ve "   " (sadece boşluk) kabul etmez
            .MaximumLength(100).WithMessage("Kullanıcı adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            // Üst sınır neden? Şifre kıyma (hash) işlemi bilerek yavaştır; çok uzun "şifrelerle" sunucuyu yormaya çalışana karşı.
            .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir.");
    }
}
