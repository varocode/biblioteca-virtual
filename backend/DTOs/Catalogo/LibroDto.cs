using BibliotecaAPI.Models.Entities;

namespace BibliotecaAPI.DTOs.Catalogo;

public class LibroDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string? Editorial { get; set; }
    public string? Sinopsis { get; set; }
    public string? PortadaUrl { get; set; }
    public int Stock { get; set; }
    public int Disponibles { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public string EtiquetaDisponibilidad { get; set; } = string.Empty;
    public List<string> Formatos { get; set; } = [];
    public List<string> Ubicaciones { get; set; } = [];
    public List<EjemplarDto> Ejemplares { get; set; } = [];
    public DateTime FechaRegistro { get; set; }
    public AutorDto Autor { get; set; } = new();
    public CategoriaDto Categoria { get; set; } = new();

    public static LibroDto DesdeEntidad(Libro libro) => new()
    {
        Id = libro.Id,
        Titulo = libro.Titulo,
        Isbn = libro.Isbn,
        Anio = libro.Anio,
        Editorial = libro.Editorial,
        Sinopsis = libro.Sinopsis,
        PortadaUrl = libro.PortadaUrl,
        Stock = libro.Stock,
        Disponibles = libro.Disponibles,
        EjemplaresDisponibles = libro.Ejemplares.Count(ejemplar => ejemplar.Estado == Models.Enums.EstadoEjemplar.Disponible),
        EtiquetaDisponibilidad = CrearEtiquetaDisponibilidad(libro),
        Formatos = libro.Ejemplares.Select(ejemplar => ejemplar.Tipo.ToString()).Distinct().Order().ToList(),
        Ubicaciones = libro.Ejemplares.Select(ejemplar => ejemplar.Ubicacion).Where(ubicacion => !string.IsNullOrWhiteSpace(ubicacion)).Select(ubicacion => ubicacion!).Distinct().Order().ToList(),
        Ejemplares = libro.Ejemplares.OrderBy(ejemplar => ejemplar.Codigo).Select(EjemplarDto.DesdeEntidad).ToList(),
        FechaRegistro = libro.FechaRegistro,
        Autor = AutorDto.DesdeEntidad(libro.Autor),
        Categoria = CategoriaDto.DesdeEntidad(libro.Categoria)
    };

    private static string CrearEtiquetaDisponibilidad(Libro libro)
    {
        var disponibles = libro.Ejemplares.Count(ejemplar => ejemplar.Estado == Models.Enums.EstadoEjemplar.Disponible);
        if (disponibles > 0)
        {
            return disponibles == 1 ? "1 ejemplar disponible" : $"{disponibles} ejemplares disponibles";
        }

        return libro.Ejemplares.Count == 0 ? "Sin ejemplares registrados" : "No disponible — podés reservarlo";
    }
}
