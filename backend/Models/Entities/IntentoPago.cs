using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class IntentoPago
{
    public int Id { get; set; }
    public int MultaId { get; set; }
    public Multa Multa { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public decimal Monto { get; set; }
    public EstadoIntentoPago Estado { get; set; } = EstadoIntentoPago.Creado;
    public string Referencia { get; set; } = string.Empty;
    public string? MotivoRechazo { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }
}
