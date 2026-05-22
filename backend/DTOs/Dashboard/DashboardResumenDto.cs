namespace BibliotecaAPI.DTOs.Dashboard;

public class DashboardResumenDto
{
    public int TotalLibros { get; set; }
    public int TotalUsuarios { get; set; }
    public int PrestamosActivos { get; set; }
    public int PrestamosVencidos { get; set; }
    public int ReservasActivas { get; set; }
    public int MultasPendientes { get; set; }
    public decimal MontoMultasPendientes { get; set; }
    public IReadOnlyList<DashboardItemDto> TopLibros { get; set; } = [];
    public IReadOnlyList<DashboardItemDto> UsuariosActivos { get; set; } = [];
    public IReadOnlyList<DashboardItemDto> PrestamosPorMes { get; set; } = [];
    public IReadOnlyList<DashboardItemDto> CategoriasPopulares { get; set; } = [];
}
