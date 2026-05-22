using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Usuarios;

namespace BibliotecaAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UsuarioDto?> ObtenerUsuarioActualAsync(int usuarioId);
}
