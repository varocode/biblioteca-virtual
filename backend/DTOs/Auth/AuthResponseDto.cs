using BibliotecaAPI.DTOs.Usuarios;

namespace BibliotecaAPI.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}
