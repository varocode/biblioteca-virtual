using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Circulacion;

public class PrestamoDto
{
    public int Id { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
    public LibroDto Libro { get; set; } = new();
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public EstadoPrestamo Estado { get; set; }
    public string? Observaciones { get; set; }
    public EjemplarDto? Ejemplar { get; set; }
    public MultaDto? Multa { get; set; }

    public static PrestamoDto DesdeEntidad(Prestamo prestamo) => new()
    {
        Id = prestamo.Id,
        Usuario = UsuarioDto.DesdeEntidad(prestamo.Usuario),
        Libro = LibroDto.DesdeEntidad(prestamo.Libro),
        FechaPrestamo = prestamo.FechaPrestamo,
        FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada,
        FechaDevolucionReal = prestamo.FechaDevolucionReal,
        Estado = prestamo.Estado,
        Observaciones = prestamo.Observaciones,
        Ejemplar = prestamo.Ejemplar is null ? null : EjemplarDto.DesdeEntidad(prestamo.Ejemplar),
        Multa = prestamo.Multa is null ? null : MultaDto.DesdeEntidad(prestamo.Multa)
    };
}
