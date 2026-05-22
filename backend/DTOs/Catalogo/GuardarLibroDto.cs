using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Catalogo;

public class GuardarLibroDto
{
    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Isbn { get; set; } = string.Empty;

    [Range(1, 3000)]
    public int Anio { get; set; }

    [MaxLength(100)]
    public string? Editorial { get; set; }

    public string? Sinopsis { get; set; }

    [MaxLength(500)]
    public string? PortadaUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 1;

    [Range(0, int.MaxValue)]
    public int Disponibles { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue)]
    public int AutorId { get; set; }
}
