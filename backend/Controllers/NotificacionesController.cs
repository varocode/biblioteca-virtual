using System.Security.Claims;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/notificaciones")]
public class NotificacionesController(ICirculacionService circulacionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await circulacionService.ListarNotificacionesAsync(ObtenerUsuarioId()!.Value));

    private int? ObtenerUsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
