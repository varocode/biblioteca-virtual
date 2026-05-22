namespace BibliotecaAPI.Models.Entities;

public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string? Editorial { get; set; }
    public string? Sinopsis { get; set; }
    public string? PortadaUrl { get; set; }
    public int Stock { get; set; } = 1;
    public int Disponibles { get; set; } = 1;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public int AutorId { get; set; }
    public Autor Autor { get; set; } = null!;

    public ICollection<Prestamo> Prestamos { get; set; } = [];
    public ICollection<Reserva> Reservas { get; set; } = [];
    public ICollection<Ejemplar> Ejemplares { get; set; } = [];
}
