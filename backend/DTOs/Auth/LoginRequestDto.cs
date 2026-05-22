using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
