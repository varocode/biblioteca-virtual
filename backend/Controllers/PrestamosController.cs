using System.Security.Claims;
using BibliotecaAPI.DTOs.Circulacion;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/prestamos")]
public class PrestamosController(ICirculacionService circulacionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await circulacionService.ListarPrestamosAsync(ObtenerUsuarioId()!.Value, EsAdmin()));

    [HttpPost]
    public async Task<IActionResult> Crear(CrearPrestamoDto request)
    {
        try
        {
            var prestamo = await circulacionService.CrearPrestamoAsync(ObtenerUsuarioId()!.Value, request);
            return CreatedAtAction(nameof(Listar), new { id = prestamo.Id }, prestamo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/aprobar")]
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> Aprobar(int id)
    {
        try
        {
            var prestamo = await circulacionService.AprobarPrestamoAsync(id, ObtenerUsuarioId()!.Value);
            return prestamo is null ? NotFound(new { mensaje = "Solicitud de préstamo no encontrada." }) : Ok(prestamo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/devolver")]
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> Devolver(int id)
    {
        try
        {
            var prestamo = await circulacionService.DevolverAsync(id, ObtenerUsuarioId()!.Value, EsAdmin());
            return prestamo is null ? NotFound(new { mensaje = "Préstamo no encontrado." }) : Ok(prestamo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/renovar")]
    public async Task<IActionResult> Renovar(int id)
    {
        try
        {
            var prestamo = await circulacionService.RenovarAsync(id, ObtenerUsuarioId()!.Value);
            return prestamo is null ? NotFound(new { mensaje = "Préstamo no encontrado." }) : Ok(prestamo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    private int? ObtenerUsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool EsAdmin() => User.IsInRole(RolUsuario.Administrador.ToString());
}
