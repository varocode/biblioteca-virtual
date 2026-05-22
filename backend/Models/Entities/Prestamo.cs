using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Prestamo
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int LibroId { get; set; }
    public Libro Libro { get; set; } = null!;
    public int? EjemplarId { get; set; }
    public Ejemplar? Ejemplar { get; set; }
    public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;
    public DateTime FechaDevolucionEsperada { get; set; } = DateTime.UtcNow.AddDays(14);
    public DateTime? FechaDevolucionReal { get; set; }
    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;
    public string? Observaciones { get; set; }

    public Multa? Multa { get; set; }
}
