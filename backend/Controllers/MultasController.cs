using System.Security.Claims;
using BibliotecaAPI.DTOs.Circulacion;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/multas")]
public class MultasController(ICirculacionService circulacionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await circulacionService.ListarMultasAsync(ObtenerUsuarioId()!.Value, EsAdmin()));

    [HttpPost("{id:int}/pagar")]
    public async Task<IActionResult> Pagar(int id, ProcesarPagoMultaDto request)
    {
        try
        {
            var intento = await circulacionService.ProcesarPagoMultaAsync(id, ObtenerUsuarioId()!.Value, EsAdmin(), request);
            return intento is null ? NotFound(new { mensaje = "Multa no encontrada." }) : Ok(intento);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    private int? ObtenerUsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool EsAdmin() => User.IsInRole(RolUsuario.Administrador.ToString());
}
