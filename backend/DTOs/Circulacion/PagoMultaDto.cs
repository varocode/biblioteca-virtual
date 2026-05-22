using BibliotecaAPI.Models.Entities;

namespace BibliotecaAPI.DTOs.Circulacion;

public class PagoMultaDto
{
    public int Id { get; set; }
    public int MultaId { get; set; }
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Recibo { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }

    public static PagoMultaDto DesdeEntidad(PagoMulta pago) => new()
    {
        Id = pago.Id,
        MultaId = pago.MultaId,
        Monto = pago.Monto,
        Referencia = pago.Referencia,
        Recibo = pago.Recibo,
        FechaPago = pago.FechaPago
    };
}
