using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize(Roles = nameof(RolUsuario.Administrador))]
[Route("api/ejemplares")]
public class EjemplaresController(BibliotecaContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? libroId)
    {
        var query = context.Ejemplares.AsNoTracking().Include(ejemplar => ejemplar.Libro).OrderBy(ejemplar => ejemplar.Libro.Titulo).ThenBy(ejemplar => ejemplar.Codigo).AsQueryable();
        if (libroId.HasValue)
        {
            query = query.Where(ejemplar => ejemplar.LibroId == libroId.Value);
        }

        var ejemplares = await query.Select(ejemplar => new
        {
            ejemplar.Id,
            ejemplar.LibroId,
            libroTitulo = ejemplar.Libro.Titulo,
            detalle = EjemplarDto.DesdeEntidad(ejemplar)
        }).ToListAsync();

        return Ok(ejemplares);
    }
}
