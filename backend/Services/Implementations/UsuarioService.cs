using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Services.Implementations;

public class UsuarioService(BibliotecaContext context) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync() =>
        await context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .Select(usuario => new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                FechaRegistro = usuario.FechaRegistro
            })
            .ToListAsync();

    public async Task<UsuarioDto?> ObtenerAsync(int id)
    {
        var usuario = await context.Usuarios.AsNoTracking().SingleOrDefaultAsync(usuario => usuario.Id == id);
        return usuario is null ? null : UsuarioDto.DesdeEntidad(usuario);
    }

    public async Task<UsuarioDto?> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDto request)
    {
        var usuario = await context.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == usuarioId && usuario.Activo);
        if (usuario is null)
        {
            return null;
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Telefono = NormalizarOpcional(request.Telefono);
        usuario.Direccion = NormalizarOpcional(request.Direccion);
        await context.SaveChangesAsync();

        return UsuarioDto.DesdeEntidad(usuario);
    }

    public async Task<bool> CambiarPasswordAsync(int usuarioId, CambiarPasswordDto request)
    {
        var usuario = await context.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == usuarioId && usuario.Activo);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash))
        {
            return false;
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNuevo);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<UsuarioDto?> ActualizarAdminAsync(int id, ActualizarUsuarioAdminDto request)
    {
        var usuario = await context.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == id);
        if (usuario is null)
        {
            return null;
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Rol = request.Rol;
        usuario.Activo = request.Activo;
        usuario.Telefono = NormalizarOpcional(request.Telefono);
        usuario.Direccion = NormalizarOpcional(request.Direccion);
        await context.SaveChangesAsync();

        return UsuarioDto.DesdeEntidad(usuario);
    }

    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
