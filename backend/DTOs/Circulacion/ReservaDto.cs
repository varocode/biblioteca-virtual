using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Circulacion;

public class ReservaDto
{
    public int Id { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
    public LibroDto Libro { get; set; } = new();
    public DateTime FechaReserva { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int PosicionCola { get; set; }
    public EstadoReserva Estado { get; set; }
    public EjemplarDto? Ejemplar { get; set; }

    public static ReservaDto DesdeEntidad(Reserva reserva) => new()
    {
        Id = reserva.Id,
        Usuario = UsuarioDto.DesdeEntidad(reserva.Usuario),
        Libro = LibroDto.DesdeEntidad(reserva.Libro),
        FechaReserva = reserva.FechaReserva,
        FechaExpiracion = reserva.FechaExpiracion,
        PosicionCola = reserva.PosicionCola,
        Estado = reserva.Estado,
        Ejemplar = reserva.Ejemplar is null ? null : EjemplarDto.DesdeEntidad(reserva.Ejemplar)
    };
}
