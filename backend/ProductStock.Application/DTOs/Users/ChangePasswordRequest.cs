namespace ProductStock.Application.DTOs.Users;

// Kendi şifreni değiştirme formu.
public class ChangePasswordRequest
{
    // Mevcut şifre: "gerçekten sen misin?" kontrolü (açık bırakılan bilgisayara karşı).
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
