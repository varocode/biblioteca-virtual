using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Circulacion;

public class IntentoPagoDto
{
    public int Id { get; set; }
    public int MultaId { get; set; }
    public decimal Monto { get; set; }
    public EstadoIntentoPago Estado { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string? MotivoRechazo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }

    public static IntentoPagoDto DesdeEntidad(IntentoPago intento) => new()
    {
        Id = intento.Id,
        MultaId = intento.MultaId,
        Monto = intento.Monto,
        Estado = intento.Estado,
        Referencia = intento.Referencia,
        MotivoRechazo = intento.MotivoRechazo,
        FechaCreacion = intento.FechaCreacion,
        FechaResolucion = intento.FechaResolucion
    };
}
