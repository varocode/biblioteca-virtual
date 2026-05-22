using System.Security.Claims;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Registrar(RegisterRequestDto request)
    {
        try
        {
            return Created("/api/auth/me", await authService.RegistrarAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        try
        {
            return Ok(await authService.LoginAsync(request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var usuario = await authService.ObtenerUsuarioActualAsync(usuarioId.Value);
        return usuario is null ? Unauthorized(new { mensaje = "Usuario no disponible." }) : Ok(usuario);
    }

    private int? ObtenerUsuarioId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
