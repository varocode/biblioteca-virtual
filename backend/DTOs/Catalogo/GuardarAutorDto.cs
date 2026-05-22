using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Catalogo;

public class GuardarAutorDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Nacionalidad { get; set; }

    public string? Biografia { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
}
