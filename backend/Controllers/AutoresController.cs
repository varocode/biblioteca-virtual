using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/autores")]
public class AutoresController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await catalogoService.ListarAutoresAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var autor = await catalogoService.ObtenerAutorAsync(id);
        return autor is null ? NotFound(new { mensaje = "Autor no encontrado." }) : Ok(autor);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost]
    public async Task<IActionResult> Crear(GuardarAutorDto request)
    {
        var autor = await catalogoService.CrearAutorAsync(request);
        return CreatedAtAction(nameof(Obtener), new { id = autor.Id }, autor);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, GuardarAutorDto request)
    {
        var autor = await catalogoService.ActualizarAutorAsync(id, request);
        return autor is null ? NotFound(new { mensaje = "Autor no encontrado." }) : Ok(autor);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await catalogoService.EliminarAutorAsync(id) ? NoContent() : NotFound(new { mensaje = "Autor no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
