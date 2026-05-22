using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Ejemplar
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public EstadoEjemplar Estado { get; set; } = EstadoEjemplar.Disponible;
    public TipoEjemplar Tipo { get; set; } = TipoEjemplar.Fisico;
    public string? Ubicacion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public int LibroId { get; set; }
    public Libro Libro { get; set; } = null!;

    public ICollection<Prestamo> Prestamos { get; set; } = [];
    public ICollection<Reserva> Reservas { get; set; } = [];
}
