using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int LibroId { get; set; }
    public Libro Libro { get; set; } = null!;
    public int? EjemplarId { get; set; }
    public Ejemplar? Ejemplar { get; set; }
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;
    public DateTime? FechaExpiracion { get; set; }
    public int PosicionCola { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Activa;
}
