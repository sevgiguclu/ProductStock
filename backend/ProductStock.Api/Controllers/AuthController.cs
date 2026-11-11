using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductStock.Application.DTOs.Auth;
using ProductStock.Application.DTOs.Users;
using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Controllers;

// Giriş "kapısı": kullanıcı adı + şifre karşılığında bileklik (token) verir.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    // POST api/auth/login   Gövde: { "username": "admin", "password": "..." }
    // Başarılı → 200 + token. Hatalı → 401 "Kullanıcı adı veya şifre hatalı."
    // [AllowAnonymous]: bileklik almaya gelen kişinin henüz bilekliği yok; bu kapı kilitli olamaz.
    // Sadece bu metoda koyduk (sınıfa değil): ileride buraya eklenecek "şifre değiştir" gibi metotlar kilitli kalsın.
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    // POST api/auth/change-password   Gövde: { "currentPassword": "...", "newPassword": "..." }
    // [AllowAnonymous] YOK → "varsayılan kilitli" kural geçerli: giriş yapmış herkes (her rol) kendi şifresini değiştirebilir.
    // Başarılı → 200 + YENİ bileklik. Eski bileklikler (bu dahil) artık geçersiz; ön yüz yenisini saklamalı.
    [HttpPost("change-password")]
    public async Task<ActionResult<LoginResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await _userService.ChangeOwnPasswordAsync(request, cancellationToken);
        return Ok(response);
    }
}
