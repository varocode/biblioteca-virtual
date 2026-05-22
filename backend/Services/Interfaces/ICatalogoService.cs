using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Common;

namespace BibliotecaAPI.Services.Interfaces;

public interface ICatalogoService
{
    Task<PagedResultDto<LibroDto>> ListarLibrosAsync(LibroQueryDto query);
    Task<LibroDto?> ObtenerLibroAsync(int id);
    Task<LibroDto> CrearLibroAsync(GuardarLibroDto request);
    Task<LibroDto?> ActualizarLibroAsync(int id, GuardarLibroDto request);
    Task<bool> EliminarLibroAsync(int id);
    Task<IReadOnlyList<AutorDto>> ListarAutoresAsync();
    Task<AutorDto?> ObtenerAutorAsync(int id);
    Task<AutorDto> CrearAutorAsync(GuardarAutorDto request);
    Task<AutorDto?> ActualizarAutorAsync(int id, GuardarAutorDto request);
    Task<bool> EliminarAutorAsync(int id);
    Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync();
    Task<CategoriaDto?> ObtenerCategoriaAsync(int id);
    Task<CategoriaDto> CrearCategoriaAsync(GuardarCategoriaDto request);
    Task<CategoriaDto?> ActualizarCategoriaAsync(int id, GuardarCategoriaDto request);
    Task<bool> EliminarCategoriaAsync(int id);
}
