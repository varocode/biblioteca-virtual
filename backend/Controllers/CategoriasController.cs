using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await catalogoService.ListarCategoriasAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var categoria = await catalogoService.ObtenerCategoriaAsync(id);
        return categoria is null ? NotFound(new { mensaje = "Categoría no encontrada." }) : Ok(categoria);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost]
    public async Task<IActionResult> Crear(GuardarCategoriaDto request)
    {
        try
        {
            var categoria = await catalogoService.CrearCategoriaAsync(request);
            return CreatedAtAction(nameof(Obtener), new { id = categoria.Id }, categoria);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, GuardarCategoriaDto request)
    {
        try
        {
            var categoria = await catalogoService.ActualizarCategoriaAsync(id, request);
            return categoria is null ? NotFound(new { mensaje = "Categoría no encontrada." }) : Ok(categoria);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await catalogoService.EliminarCategoriaAsync(id) ? NoContent() : NotFound(new { mensaje = "Categoría no encontrada." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
