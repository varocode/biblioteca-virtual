using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Usuarios;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }

    public static UsuarioDto DesdeEntidad(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Email = usuario.Email,
        Rol = usuario.Rol,
        Activo = usuario.Activo,
        Telefono = usuario.Telefono,
        Direccion = usuario.Direccion,
        FechaRegistro = usuario.FechaRegistro
    };
}
