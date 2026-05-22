using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Common;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Services.Implementations;

public class CatalogoService(BibliotecaContext context) : ICatalogoService
{
    public async Task<PagedResultDto<LibroDto>> ListarLibrosAsync(LibroQueryDto query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var libros = context.Libros.AsNoTracking().Include(libro => libro.Autor).Include(libro => libro.Categoria).Include(libro => libro.Ejemplares).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var buscaDigital = "digital".Contains(search) || search.Contains("digital");
            var buscaFisico = "fisico".Contains(search) || "físico".Contains(search) || search.Contains("fisico") || search.Contains("físico");
            libros = libros.Where(libro =>
                libro.Titulo.ToLower().Contains(search) ||
                libro.Isbn.ToLower().Contains(search) ||
                (libro.Editorial != null && libro.Editorial.ToLower().Contains(search)) ||
                (libro.Sinopsis != null && libro.Sinopsis.ToLower().Contains(search)) ||
                libro.Autor.Nombre.ToLower().Contains(search) ||
                libro.Categoria.Nombre.ToLower().Contains(search) ||
                libro.Ejemplares.Any(ejemplar =>
                    ejemplar.Codigo.ToLower().Contains(search) ||
                    (buscaDigital && ejemplar.Tipo == TipoEjemplar.Digital) ||
                    (buscaFisico && ejemplar.Tipo == TipoEjemplar.Fisico) ||
                    (ejemplar.Ubicacion != null && ejemplar.Ubicacion.ToLower().Contains(search))));
        }

        if (query.AutorId is not null)
        {
            libros = libros.Where(libro => libro.AutorId == query.AutorId);
        }

        if (query.CategoriaId is not null)
        {
            libros = libros.Where(libro => libro.CategoriaId == query.CategoriaId);
        }

        if (query.Anio is not null)
        {
            libros = libros.Where(libro => libro.Anio == query.Anio);
        }

        var tieneFiltroTipo = Enum.TryParse<TipoEjemplar>(query.TipoEjemplar, true, out var tipoEjemplar);

        if (query.Disponible is true && tieneFiltroTipo)
        {
            libros = libros.Where(libro => libro.Ejemplares.Any(ejemplar => ejemplar.Tipo == tipoEjemplar && ejemplar.Estado == EstadoEjemplar.Disponible));
        }
        else if (query.Disponible is false && tieneFiltroTipo)
        {
            libros = libros.Where(libro => !libro.Ejemplares.Any(ejemplar => ejemplar.Tipo == tipoEjemplar && ejemplar.Estado == EstadoEjemplar.Disponible));
        }
        else if (query.Disponible is true)
        {
            libros = libros.Where(libro => libro.Ejemplares.Any(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible));
        }
        else if (query.Disponible is false)
        {
            libros = libros.Where(libro => !libro.Ejemplares.Any(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible));
        }

        if (tieneFiltroTipo)
        {
            libros = libros.Where(libro => libro.Ejemplares.Any(ejemplar => ejemplar.Tipo == tipoEjemplar));
        }

        if (!string.IsNullOrWhiteSpace(query.Ubicacion))
        {
            var ubicacion = query.Ubicacion.Trim().ToLower();
            libros = libros.Where(libro => libro.Ejemplares.Any(ejemplar => ejemplar.Ubicacion != null && ejemplar.Ubicacion.ToLower().Contains(ubicacion)));
        }

        libros = Ordenar(libros, query.SortBy, query.SortDir);
        var total = await libros.CountAsync();
        var items = await libros.Skip((page - 1) * pageSize).Take(pageSize).Select(libro => LibroDto.DesdeEntidad(libro)).ToListAsync();

        return new PagedResultDto<LibroDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<LibroDto?> ObtenerLibroAsync(int id)
    {
        var libro = await LibroConRelaciones().AsNoTracking().SingleOrDefaultAsync(libro => libro.Id == id);
        return libro is null ? null : LibroDto.DesdeEntidad(libro);
    }

    public async Task<LibroDto> CrearLibroAsync(GuardarLibroDto request)
    {
        await ValidarLibroAsync(request);
        if (await context.Libros.AnyAsync(libro => libro.Isbn == request.Isbn.Trim()))
        {
            throw new InvalidOperationException("Ya existe un libro con ese ISBN.");
        }

        var libro = new Libro();
        AplicarLibro(libro, request);
        SincronizarEjemplares(libro, request.Stock, request.Disponibles);
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        return (await ObtenerLibroAsync(libro.Id))!;
    }

    public async Task<LibroDto?> ActualizarLibroAsync(int id, GuardarLibroDto request)
    {
        var libro = await context.Libros.Include(libro => libro.Ejemplares).SingleOrDefaultAsync(libro => libro.Id == id);
        if (libro is null)
        {
            return null;
        }

        await ValidarLibroAsync(request);
        var isbn = request.Isbn.Trim();
        if (await context.Libros.AnyAsync(libro => libro.Id != id && libro.Isbn == isbn))
        {
            throw new InvalidOperationException("Ya existe un libro con ese ISBN.");
        }

        AplicarLibro(libro, request);
        SincronizarEjemplares(libro, request.Stock, request.Disponibles);
        await context.SaveChangesAsync();
        return (await ObtenerLibroAsync(id))!;
    }

    public async Task<bool> EliminarLibroAsync(int id)
    {
        var libro = await context.Libros.SingleOrDefaultAsync(libro => libro.Id == id);
        if (libro is null)
        {
            return false;
        }

        context.Libros.Remove(libro);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<AutorDto>> ListarAutoresAsync() =>
        await context.Autores.AsNoTracking().OrderBy(autor => autor.Nombre).Select(autor => AutorDto.DesdeEntidad(autor)).ToListAsync();

    public async Task<AutorDto?> ObtenerAutorAsync(int id)
    {
        var autor = await context.Autores.AsNoTracking().SingleOrDefaultAsync(autor => autor.Id == id);
        return autor is null ? null : AutorDto.DesdeEntidad(autor);
    }

    public async Task<AutorDto> CrearAutorAsync(GuardarAutorDto request)
    {
        var autor = new Autor();
        AplicarAutor(autor, request);
        context.Autores.Add(autor);
        await context.SaveChangesAsync();
        return AutorDto.DesdeEntidad(autor);
    }

    public async Task<AutorDto?> ActualizarAutorAsync(int id, GuardarAutorDto request)
    {
        var autor = await context.Autores.SingleOrDefaultAsync(autor => autor.Id == id);
        if (autor is null)
        {
            return null;
        }

        AplicarAutor(autor, request);
        await context.SaveChangesAsync();
        return AutorDto.DesdeEntidad(autor);
    }

    public async Task<bool> EliminarAutorAsync(int id)
    {
        if (await context.Libros.AnyAsync(libro => libro.AutorId == id))
        {
            throw new InvalidOperationException("No se puede eliminar un autor con libros asociados.");
        }

        var autor = await context.Autores.SingleOrDefaultAsync(autor => autor.Id == id);
        if (autor is null)
        {
            return false;
        }

        context.Autores.Remove(autor);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync() =>
        await context.Categorias.AsNoTracking().OrderBy(categoria => categoria.Nombre).Select(categoria => CategoriaDto.DesdeEntidad(categoria)).ToListAsync();

    public async Task<CategoriaDto?> ObtenerCategoriaAsync(int id)
    {
        var categoria = await context.Categorias.AsNoTracking().SingleOrDefaultAsync(categoria => categoria.Id == id);
        return categoria is null ? null : CategoriaDto.DesdeEntidad(categoria);
    }

    public async Task<CategoriaDto> CrearCategoriaAsync(GuardarCategoriaDto request)
    {
        await ValidarCategoriaUnicaAsync(request.Nombre);
        var categoria = new Categoria();
        AplicarCategoria(categoria, request);
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        return CategoriaDto.DesdeEntidad(categoria);
    }

    public async Task<CategoriaDto?> ActualizarCategoriaAsync(int id, GuardarCategoriaDto request)
    {
        var categoria = await context.Categorias.SingleOrDefaultAsync(categoria => categoria.Id == id);
        if (categoria is null)
        {
            return null;
        }

        await ValidarCategoriaUnicaAsync(request.Nombre, id);
        AplicarCategoria(categoria, request);
        await context.SaveChangesAsync();
        return CategoriaDto.DesdeEntidad(categoria);
    }

    public async Task<bool> EliminarCategoriaAsync(int id)
    {
        if (await context.Libros.AnyAsync(libro => libro.CategoriaId == id))
        {
            throw new InvalidOperationException("No se puede eliminar una categoría con libros asociados.");
        }

        var categoria = await context.Categorias.SingleOrDefaultAsync(categoria => categoria.Id == id);
        if (categoria is null)
        {
            return false;
        }

        context.Categorias.Remove(categoria);
        await context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Libro> LibroConRelaciones() => context.Libros.Include(libro => libro.Autor).Include(libro => libro.Categoria).Include(libro => libro.Ejemplares);

    private static IQueryable<Libro> Ordenar(IQueryable<Libro> libros, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "anio" => desc ? libros.OrderByDescending(libro => libro.Anio) : libros.OrderBy(libro => libro.Anio),
            "autor" => desc ? libros.OrderByDescending(libro => libro.Autor.Nombre) : libros.OrderBy(libro => libro.Autor.Nombre),
            "categoria" => desc ? libros.OrderByDescending(libro => libro.Categoria.Nombre) : libros.OrderBy(libro => libro.Categoria.Nombre),
            "editorial" => desc ? libros.OrderByDescending(libro => libro.Editorial) : libros.OrderBy(libro => libro.Editorial),
            "disponibles" or "disponibilidad" => desc ? libros.OrderByDescending(libro => libro.Ejemplares.Count(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible)) : libros.OrderBy(libro => libro.Ejemplares.Count(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible)),
            "recientes" or "fecha" => desc ? libros.OrderByDescending(libro => libro.FechaRegistro) : libros.OrderBy(libro => libro.FechaRegistro),
            _ => desc ? libros.OrderByDescending(libro => libro.Titulo) : libros.OrderBy(libro => libro.Titulo)
        };
    }

    private async Task ValidarLibroAsync(GuardarLibroDto request)
    {
        if (request.Disponibles > request.Stock)
        {
            throw new ArgumentException("Los disponibles no pueden superar el stock.");
        }

        if (!await context.Autores.AnyAsync(autor => autor.Id == request.AutorId))
        {
            throw new ArgumentException("Autor no encontrado.");
        }

        if (!await context.Categorias.AnyAsync(categoria => categoria.Id == request.CategoriaId))
        {
            throw new ArgumentException("Categoría no encontrada.");
        }
    }

    private async Task ValidarCategoriaUnicaAsync(string nombre, int? id = null)
    {
        var normalizado = nombre.Trim();
        if (await context.Categorias.AnyAsync(categoria => categoria.Nombre == normalizado && categoria.Id != id))
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }
    }

    private static void AplicarLibro(Libro libro, GuardarLibroDto request)
    {
        libro.Titulo = request.Titulo.Trim();
        libro.Isbn = request.Isbn.Trim();
        libro.Anio = request.Anio;
        libro.Editorial = NormalizarOpcional(request.Editorial);
        libro.Sinopsis = NormalizarOpcional(request.Sinopsis);
        libro.PortadaUrl = NormalizarOpcional(request.PortadaUrl);
        libro.Stock = request.Stock;
        libro.Disponibles = request.Disponibles;
        libro.CategoriaId = request.CategoriaId;
        libro.AutorId = request.AutorId;
    }

    private static void SincronizarEjemplares(Libro libro, int stockSolicitado, int disponiblesSolicitados)
    {
        var actuales = libro.Ejemplares.OrderBy(ejemplar => ejemplar.Id).ThenBy(ejemplar => ejemplar.Codigo).ToList();
        for (var indice = actuales.Count + 1; indice <= stockSolicitado; indice++)
        {
            libro.Ejemplares.Add(new Ejemplar
            {
                Codigo = CrearCodigoEjemplar(libro.Isbn, indice),
                Estado = EstadoEjemplar.Prestado,
                Tipo = indice == stockSolicitado && stockSolicitado > 3 ? TipoEjemplar.Digital : TipoEjemplar.Fisico,
                Ubicacion = $"Estante {((indice - 1) % 5) + 1}"
            });
        }

        actuales = libro.Ejemplares.OrderBy(ejemplar => ejemplar.Id).ThenBy(ejemplar => ejemplar.Codigo).ToList();
        foreach (var ejemplar in actuales.Skip(stockSolicitado).Where(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible).ToList())
        {
            libro.Ejemplares.Remove(ejemplar);
        }

        actuales = libro.Ejemplares.OrderBy(ejemplar => ejemplar.Id).ThenBy(ejemplar => ejemplar.Codigo).Take(stockSolicitado).ToList();
        for (var indice = 0; indice < actuales.Count; indice++)
        {
            actuales[indice].Estado = indice < disponiblesSolicitados ? EstadoEjemplar.Disponible : EstadoEjemplar.Prestado;
            actuales[indice].Codigo = string.IsNullOrWhiteSpace(actuales[indice].Codigo) ? CrearCodigoEjemplar(libro.Isbn, indice + 1) : actuales[indice].Codigo;
        }

        RecalcularResumen(libro);
    }

    private static void RecalcularResumen(Libro libro)
    {
        libro.Stock = libro.Ejemplares.Count;
        libro.Disponibles = libro.Ejemplares.Count(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
    }

    private static string CrearCodigoEjemplar(string isbn, int indice) => $"{isbn.Trim()}-{indice:D3}";

    private static void AplicarAutor(Autor autor, GuardarAutorDto request)
    {
        autor.Nombre = request.Nombre.Trim();
        autor.Nacionalidad = NormalizarOpcional(request.Nacionalidad);
        autor.Biografia = NormalizarOpcional(request.Biografia);
        autor.FechaNacimiento = request.FechaNacimiento;
    }

    private static void AplicarCategoria(Categoria categoria, GuardarCategoriaDto request)
    {
        categoria.Nombre = request.Nombre.Trim();
        categoria.Descripcion = NormalizarOpcional(request.Descripcion);
    }

    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
