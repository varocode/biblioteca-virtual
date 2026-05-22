using BibliotecaAPI.DTOs.Usuarios;

namespace BibliotecaAPI.Services.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync();
    Task<UsuarioDto?> ObtenerAsync(int id);
    Task<UsuarioDto?> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDto request);
    Task<bool> CambiarPasswordAsync(int usuarioId, CambiarPasswordDto request);
    Task<UsuarioDto?> ActualizarAdminAsync(int id, ActualizarUsuarioAdminDto request);
}
