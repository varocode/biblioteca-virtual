using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/libros")]
public class LibrosController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] LibroQueryDto query) => Ok(await catalogoService.ListarLibrosAsync(query));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var libro = await catalogoService.ObtenerLibroAsync(id);
        return libro is null ? NotFound(new { mensaje = "Libro no encontrado." }) : Ok(libro);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost]
    public async Task<IActionResult> Crear(GuardarLibroDto request)
    {
        try
        {
            var libro = await catalogoService.CrearLibroAsync(request);
            return CreatedAtAction(nameof(Obtener), new { id = libro.Id }, libro);
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

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, GuardarLibroDto request)
    {
        try
        {
            var libro = await catalogoService.ActualizarLibroAsync(id, request);
            return libro is null ? NotFound(new { mensaje = "Libro no encontrado." }) : Ok(libro);
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

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) => await catalogoService.EliminarLibroAsync(id)
        ? NoContent()
        : NotFound(new { mensaje = "Libro no encontrado." });
}
