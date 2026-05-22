using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Circulacion;

public class NotificacionDto
{
    public int Id { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public DateTime Fecha { get; set; }

    public static NotificacionDto DesdeEntidad(Notificacion notificacion) => new()
    {
        Id = notificacion.Id,
        Tipo = notificacion.Tipo,
        Titulo = notificacion.Titulo,
        Mensaje = notificacion.Mensaje,
        Referencia = notificacion.Referencia,
        Fecha = notificacion.Fecha
    };
}
