using FluentValidation;
using ProductStock.Application.DTOs.Users;

namespace ProductStock.Application.Validators.Users;

// Şifre uzunluk sınırları TEK yerde: kullanıcı ekleme, şifre değiştirme ve giriş formu buradan okur.
// "En az 10 olsun" dersek sadece bu satır değişir.
public static class PasswordRules
{
    public const int MinLength = 8;
    // Üst sınır: şifre kıyma bilerek yavaştır; çok uzun "şifrelerle" sunucuyu yormaya çalışana karşı.
    public const int MaxLength = 128;
}

// Yeni kullanıcı formu (Admin).
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // Kontrol, servisin kaydedeceği hal (boşluk sil + küçük harf) üzerinde: "Depo1 " kabul edilir → "depo1".
        // Kalıp: küçük harf (a-z), rakam, nokta, alt çizgi, tire; 3-32 karakter.
        // Türkçe harf ve boşluk yok: giriş adı her klavyede sorunsuz yazılabilsin.
        RuleFor(x => (x.Username ?? string.Empty).Trim().ToLowerInvariant())
            .Matches("^[a-z0-9._-]{3,32}$")
            .WithMessage("Kullanıcı adı 3-32 karakter olmalı ve sadece küçük harf (a-z), rakam, nokta, alt çizgi, tire içermeli.")
            .OverridePropertyName("username");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad boş olamaz.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

        // Şifrenin başındaki/sonundaki boşluklara dokunmuyoruz: boşluk da şifrenin parçası olabilir.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .Length(PasswordRules.MinLength, PasswordRules.MaxLength)
            .WithMessage($"Şifre en az {PasswordRules.MinLength}, en fazla {PasswordRules.MaxLength} karakter olmalı.");

        // IsInEnum: değer gerçekten tanımlı rollerden biri mi? JSON'da "role": 5 gibi bir sayı gelirse yakalar.
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçersiz rol. Geçerli değerler: Admin, WarehouseStaff.");
    }
}

// Aktif/pasif formu (Admin): { "isActive": false }
public class SetUserStatusRequestValidator : AbstractValidator<SetUserStatusRequest>
{
    public SetUserStatusRequestValidator()
    {
        // NotNull: alan hiç gönderilmediyse reddet. (Düz bool olsaydı varsayılan false olur,
        // kullanıcı YANLIŞLIKLA pasif yapılırdı; bu yüzden alan bool? ve burada zorunlu.)
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("isActive alanı zorunlu (true veya false).");
    }
}

// Kendi şifreni değiştirme formu.
// Not: "Mevcut şifre DOĞRU mu?" burada kontrol edilemez (veritabanındaki kıymaya bakmak gerekir) → serviste kalır.
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre boş olamaz.")
            .MaximumLength(PasswordRules.MaxLength).WithMessage($"Mevcut şifre en fazla {PasswordRules.MaxLength} karakter olabilir.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş olamaz.")
            .Length(PasswordRules.MinLength, PasswordRules.MaxLength)
            .WithMessage($"Yeni şifre en az {PasswordRules.MinLength}, en fazla {PasswordRules.MaxLength} karakter olmalı.")
            // NotEqual(x => x.CurrentPassword): iki alanı karşılaştıran kural. Formun içinden bakılabildiği için doğrulamaya ait.
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifreyle aynı olamaz.");
    }
}
