using System.Security.Claims;
using BibliotecaAPI.DTOs.Circulacion;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/reservas")]
public class ReservasController(ICirculacionService circulacionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await circulacionService.ListarReservasAsync(ObtenerUsuarioId()!.Value, EsAdmin()));

    [HttpPost]
    public async Task<IActionResult> Crear(CrearReservaDto request)
    {
        try
        {
            var reserva = await circulacionService.CrearReservaAsync(ObtenerUsuarioId()!.Value, request);
            return CreatedAtAction(nameof(Listar), new { id = reserva.Id }, reserva);
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

    [HttpPost("{id:int}/preparar-retiro")]
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> PrepararRetiro(int id)
    {
        try
        {
            var reserva = await circulacionService.PrepararRetiroReservaAsync(id);
            return reserva is null ? NotFound(new { mensaje = "Reserva no encontrada." }) : Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            return await circulacionService.CancelarReservaAsync(id, ObtenerUsuarioId()!.Value, EsAdmin())
                ? NoContent()
                : NotFound(new { mensaje = "Reserva no encontrada." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    private int? ObtenerUsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool EsAdmin() => User.IsInRole(RolUsuario.Administrador.ToString());
}
