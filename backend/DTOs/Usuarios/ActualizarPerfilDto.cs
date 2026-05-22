using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Usuarios;

public class ActualizarPerfilDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(255)]
    public string? Direccion { get; set; }
}
