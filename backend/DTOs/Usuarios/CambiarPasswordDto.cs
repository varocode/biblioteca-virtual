using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Usuarios;

public class CambiarPasswordDto
{
    [Required]
    public string PasswordActual { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string PasswordNuevo { get; set; } = string.Empty;
}
