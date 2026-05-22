using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public TipoNotificacion Tipo { get; set; } = TipoNotificacion.Prestamo;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
