using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductStock.Api.Security;
using ProductStock.Application.DTOs.Users;
using ProductStock.Application.Interfaces;

namespace ProductStock.Api.Controllers;

// Kullanıcı yönetimi. Adresi: api/users
// [Authorize] sınıfın TEPESİNDE: içindeki BÜTÜN metotlar sadece Admin'e açık.
// (ProductsController'da metot metot koymuştuk çünkü orada okuma herkese açıktı.)
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET api/users → tüm kullanıcılar (şifre kıyması olmadan)
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    // GET api/users/{id} → tek kullanıcı
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByIdAsync(id, cancellationToken));
    }

    // POST api/users → yeni kullanıcı. 201 Created + yeni kullanıcının adresi.
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/users/{id}/status   Gövde: { "isActive": false }
    // Silmek yerine pasif yapıyoruz: geçmiş stok fişlerindeki "işlemi yapan" bilgisi anlamlı kalsın.
    // PUT: "durumu şuna AYARLA". Aynı isteği iki kez göndermek sonucu değiştirmez (idempotent).
    [HttpPut("{id}/status")]
    public async Task<ActionResult<UserResponse>> SetStatus(string id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.SetStatusAsync(id, request, cancellationToken));
    }
}
