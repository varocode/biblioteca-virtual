using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Lector;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    public ICollection<Prestamo> Prestamos { get; set; } = [];
    public ICollection<Reserva> Reservas { get; set; } = [];
    public ICollection<Multa> Multas { get; set; } = [];
    public ICollection<Notificacion> Notificaciones { get; set; } = [];
}
