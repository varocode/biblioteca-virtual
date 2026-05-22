using BibliotecaAPI.DTOs.Dashboard;

namespace BibliotecaAPI.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync();
}
