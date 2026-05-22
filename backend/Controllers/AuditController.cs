using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize(Roles = nameof(RolUsuario.Administrador))]
[Route("api/audit")]
public class AuditController(ICirculacionService circulacionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? entidad, [FromQuery] int? usuarioId) => Ok(await circulacionService.ListarAuditoriaAsync(entidad, usuarioId));

    [HttpPut("{id:int}")]
    public IActionResult ModificarBloqueado(int id) => StatusCode(StatusCodes.Status405MethodNotAllowed, new { mensaje = "La auditoría es append-only y no se puede modificar." });

    [HttpDelete("{id:int}")]
    public IActionResult EliminarBloqueado(int id) => StatusCode(StatusCodes.Status405MethodNotAllowed, new { mensaje = "La auditoría es append-only y no se puede eliminar." });
}
