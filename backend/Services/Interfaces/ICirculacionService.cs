using BibliotecaAPI.DTOs.Circulacion;

namespace BibliotecaAPI.Services.Interfaces;

public interface ICirculacionService
{
    Task<IReadOnlyList<PrestamoDto>> ListarPrestamosAsync(int usuarioId, bool esAdmin);
    Task<PrestamoDto> CrearPrestamoAsync(int usuarioId, CrearPrestamoDto request);
    Task<PrestamoDto?> AprobarPrestamoAsync(int prestamoId, int adminId);
    Task<PrestamoDto?> DevolverAsync(int prestamoId, int usuarioId, bool esAdmin);
    Task<PrestamoDto?> RenovarAsync(int prestamoId, int usuarioId);
    Task<IReadOnlyList<ReservaDto>> ListarReservasAsync(int usuarioId, bool esAdmin);
    Task<ReservaDto> CrearReservaAsync(int usuarioId, CrearReservaDto request);
    Task<ReservaDto?> PrepararRetiroReservaAsync(int reservaId);
    Task<bool> CancelarReservaAsync(int reservaId, int usuarioId, bool esAdmin);
    Task<IReadOnlyList<MultaDto>> ListarMultasAsync(int usuarioId, bool esAdmin);
    Task<IntentoPagoDto?> ProcesarPagoMultaAsync(int multaId, int usuarioId, bool esAdmin, ProcesarPagoMultaDto request);
    Task<IReadOnlyList<AuditEventDto>> ListarAuditoriaAsync(string? entidad, int? usuarioId);
    Task<IReadOnlyList<NotificacionDto>> ListarNotificacionesAsync(int usuarioId);
    Task MarcarVencidosYExpirarReservasAsync();
}
