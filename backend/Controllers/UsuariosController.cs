using System.Security.Claims;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await usuarioService.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        if (!EsAdmin() && ObtenerUsuarioId() != id)
        {
            return Forbid();
        }

        var usuario = await usuarioService.ObtenerAsync(id);
        return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPut("me")]
    public async Task<IActionResult> ActualizarPerfil(ActualizarPerfilDto request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var usuario = await usuarioService.ActualizarPerfilAsync(usuarioId.Value, request);
        return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var actualizado = await usuarioService.CambiarPasswordAsync(usuarioId.Value, request);
        return actualizado ? NoContent() : BadRequest(new { mensaje = "La contraseña actual no es válida." });
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarAdmin(int id, ActualizarUsuarioAdminDto request)
    {
        var usuario = await usuarioService.ActualizarAdminAsync(id, request);
        return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
    }

    private int? ObtenerUsuarioId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private bool EsAdmin() => User.IsInRole(RolUsuario.Administrador.ToString());
}
