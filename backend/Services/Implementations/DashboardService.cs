using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Dashboard;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Services.Implementations;

public class DashboardService(BibliotecaContext context, ICirculacionService circulacionService) : IDashboardService
{
    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        await circulacionService.MarcarVencidosYExpirarReservasAsync();

        return new DashboardResumenDto
        {
            TotalLibros = await context.Libros.CountAsync(),
            TotalUsuarios = await context.Usuarios.CountAsync(),
            PrestamosActivos = await context.Prestamos.CountAsync(prestamo => prestamo.Estado == EstadoPrestamo.Activo),
            PrestamosVencidos = await context.Prestamos.CountAsync(prestamo => prestamo.Estado == EstadoPrestamo.Vencido),
            ReservasActivas = await context.Reservas.CountAsync(reserva => reserva.Estado == EstadoReserva.Activa || reserva.Estado == EstadoReserva.Asignada),
            MultasPendientes = await context.Multas.CountAsync(multa => multa.Estado == EstadoMulta.Pendiente),
            MontoMultasPendientes = await context.Multas.Where(multa => multa.Estado == EstadoMulta.Pendiente).SumAsync(multa => multa.Monto),
            TopLibros = await TopLibrosAsync(),
            UsuariosActivos = await UsuariosActivosAsync(),
            PrestamosPorMes = await PrestamosPorMesAsync(),
            CategoriasPopulares = await CategoriasPopularesAsync()
        };
    }

    private async Task<IReadOnlyList<DashboardItemDto>> TopLibrosAsync() => await context.Prestamos.AsNoTracking().GroupBy(prestamo => prestamo.Libro.Titulo).OrderByDescending(group => group.Count()).Take(5).Select(group => new DashboardItemDto { Etiqueta = group.Key, Total = group.Count() }).ToListAsync();
    private async Task<IReadOnlyList<DashboardItemDto>> UsuariosActivosAsync() => await context.Prestamos.AsNoTracking().GroupBy(prestamo => prestamo.Usuario.Nombre).OrderByDescending(group => group.Count()).Take(5).Select(group => new DashboardItemDto { Etiqueta = group.Key, Total = group.Count() }).ToListAsync();
    private async Task<IReadOnlyList<DashboardItemDto>> CategoriasPopularesAsync() => await context.Prestamos.AsNoTracking().GroupBy(prestamo => prestamo.Libro.Categoria.Nombre).OrderByDescending(group => group.Count()).Take(5).Select(group => new DashboardItemDto { Etiqueta = group.Key, Total = group.Count() }).ToListAsync();
    private async Task<IReadOnlyList<DashboardItemDto>> PrestamosPorMesAsync() => await context.Prestamos.AsNoTracking().GroupBy(prestamo => new { prestamo.FechaPrestamo.Year, prestamo.FechaPrestamo.Month }).OrderBy(group => group.Key.Year).ThenBy(group => group.Key.Month).Select(group => new DashboardItemDto { Etiqueta = $"{group.Key.Year:D4}-{group.Key.Month:D2}", Total = group.Count() }).ToListAsync();
}
