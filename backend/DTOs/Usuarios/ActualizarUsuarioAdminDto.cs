using System.ComponentModel.DataAnnotations;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Usuarios;

public class ActualizarUsuarioAdminDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Lector;

    public bool Activo { get; set; } = true;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(255)]
    public string? Direccion { get; set; }
}
