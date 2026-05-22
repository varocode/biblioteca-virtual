using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Auth;

public class RegisterRequestDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(255)]
    public string? Direccion { get; set; }
}
