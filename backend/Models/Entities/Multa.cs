using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.Models.Entities;

public class Multa
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int PrestamoId { get; set; }
    public Prestamo Prestamo { get; set; } = null!;
    public decimal Monto { get; set; }
    public int DiasRetraso { get; set; }
    public EstadoMulta Estado { get; set; } = EstadoMulta.Pendiente;
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaPago { get; set; }
    public PagoMulta? Pago { get; set; }
    public ICollection<IntentoPago> IntentosPago { get; set; } = [];
}
