using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductStock.Application.DTOs.Auth;
using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Controllers;

// Giriş "kapısı": kullanıcı adı + şifre karşılığında bileklik (token) verir.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}
