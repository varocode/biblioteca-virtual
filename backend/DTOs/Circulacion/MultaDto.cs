using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Circulacion;

public class MultaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int PrestamoId { get; set; }
    public decimal Monto { get; set; }
    public int DiasRetraso { get; set; }
    public EstadoMulta Estado { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public DateTime? FechaPago { get; set; }
    public PagoMultaDto? Pago { get; set; }
    public List<IntentoPagoDto> IntentosPago { get; set; } = [];

    public static MultaDto DesdeEntidad(Multa multa) => new()
    {
        Id = multa.Id,
        UsuarioId = multa.UsuarioId,
        PrestamoId = multa.PrestamoId,
        Monto = multa.Monto,
        DiasRetraso = multa.DiasRetraso,
        Estado = multa.Estado,
        FechaGeneracion = multa.FechaGeneracion,
        FechaPago = multa.FechaPago,
        Pago = multa.Pago is null ? null : PagoMultaDto.DesdeEntidad(multa.Pago),
        IntentosPago = multa.IntentosPago.OrderByDescending(intento => intento.FechaCreacion).Select(IntentoPagoDto.DesdeEntidad).ToList()
    };
}
