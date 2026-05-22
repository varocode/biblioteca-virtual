namespace BibliotecaAPI.Models.Entities;

public class AuditEvent
{
    public int Id { get; set; }
    public int? ActorUsuarioId { get; set; }
    public Usuario? ActorUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
