using BibliotecaAPI.Models.Entities;

namespace BibliotecaAPI.DTOs.Circulacion;

public class AuditEventDto
{
    public int Id { get; set; }
    public int? ActorUsuarioId { get; set; }
    public string? ActorNombre { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; }

    public static AuditEventDto DesdeEntidad(AuditEvent audit) => new()
    {
        Id = audit.Id,
        ActorUsuarioId = audit.ActorUsuarioId,
        ActorNombre = audit.ActorUsuario?.Nombre,
        Accion = audit.Accion,
        Entidad = audit.Entidad,
        EntidadId = audit.EntidadId,
        Resultado = audit.Resultado,
        Detalle = audit.Detalle,
        Fecha = audit.Fecha
    };
}
