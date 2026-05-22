namespace BibliotecaAPI.Models.Entities;

public class PagoMulta
{
    public int Id { get; set; }
    public int MultaId { get; set; }
    public Multa Multa { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Recibo { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
}
