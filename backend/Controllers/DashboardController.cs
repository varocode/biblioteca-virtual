using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize(Roles = nameof(RolUsuario.Administrador))]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen() => Ok(await dashboardService.ObtenerResumenAsync());
}
