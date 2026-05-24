using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Circulacion;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Services.Implementations;

public class CirculacionService(BibliotecaContext context) : ICirculacionService
{
    private const int MaxPrestamosActivos = 3;
    private const int MaxReservasActivasPorLibro = 5;
    private const int DiasPrestamo = 14;
    private const int HorasRetiroReserva = 48;
    private const decimal MultaPorDia = 50m;

    public async Task<IReadOnlyList<PrestamoDto>> ListarPrestamosAsync(int usuarioId, bool esAdmin)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var query = PrestamosConRelaciones().AsNoTracking();
        if (!esAdmin)
        {
            query = query.Where(prestamo => prestamo.UsuarioId == usuarioId);
        }

        return await query.OrderByDescending(prestamo => prestamo.FechaPrestamo).Select(prestamo => PrestamoDto.DesdeEntidad(prestamo)).ToListAsync();
    }

    public async Task<PrestamoDto> CrearPrestamoAsync(int usuarioId, CrearPrestamoDto request)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var usuario = await context.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == usuarioId);
        var libroExiste = await context.Libros.AnyAsync(libro => libro.Id == request.LibroId);
        if (usuario is null || !usuario.Activo)
        {
            throw new InvalidOperationException("El usuario no está activo.");
        }
        if (!libroExiste)
        {
            throw new ArgumentException("Libro no encontrado.");
        }
        if (await TieneMultasPendientesAsync(usuarioId))
        {
            throw new InvalidOperationException("No puede solicitar préstamos con multas pendientes.");
        }
        if (await context.Prestamos.CountAsync(prestamo => prestamo.UsuarioId == usuarioId && (prestamo.Estado == EstadoPrestamo.Activo || prestamo.Estado == EstadoPrestamo.Pendiente)) >= MaxPrestamosActivos)
        {
            throw new InvalidOperationException("El usuario ya tiene 3 préstamos activos o pendientes.");
        }
        if (await context.Prestamos.AnyAsync(prestamo => prestamo.UsuarioId == usuarioId && prestamo.LibroId == request.LibroId && (prestamo.Estado == EstadoPrestamo.Activo || prestamo.Estado == EstadoPrestamo.Pendiente)))
        {
            throw new InvalidOperationException("El usuario ya tiene un préstamo activo o pendiente de este libro.");
        }

        var hoy = DateTime.UtcNow;
        var prestamo = new Prestamo
        {
            UsuarioId = usuarioId,
            LibroId = request.LibroId,
            FechaPrestamo = hoy,
            FechaDevolucionEsperada = hoy.AddDays(DiasPrestamo),
            Estado = EstadoPrestamo.Pendiente,
            Observaciones = "Solicitud pendiente de aprobación por biblioteca."
        };

        context.Prestamos.Add(prestamo);
        AgregarAuditoria(usuarioId, "prestamo.solicitar", "Prestamo", "pendiente", "Solicitud de préstamo creada.");
        AgregarNotificacion(usuarioId, TipoNotificacion.Prestamo, "Solicitud registrada", "Tu solicitud de préstamo quedó pendiente de aprobación por biblioteca.", $"prestamo:{request.LibroId}");
        await NotificarAdminsAsync(TipoNotificacion.Prestamo, "Nueva solicitud de préstamo", $"{usuario.Nombre} solicitó un préstamo. Revisa Circulación para aprobarlo.", $"prestamo:{request.LibroId}");
        await context.SaveChangesAsync();
        return (await ObtenerPrestamoDtoAsync(prestamo.Id))!;
    }

    public async Task<PrestamoDto?> AprobarPrestamoAsync(int prestamoId, int adminId)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var prestamo = await context.Prestamos
            .Include(prestamo => prestamo.Libro).ThenInclude(libro => libro.Ejemplares)
            .SingleOrDefaultAsync(prestamo => prestamo.Id == prestamoId);
        if (prestamo is null)
        {
            return null;
        }
        if (prestamo.Estado != EstadoPrestamo.Pendiente)
        {
            throw new InvalidOperationException("Solo se pueden aprobar solicitudes pendientes.");
        }

        var reservaAsignada = await ObtenerReservaAsignadaAsync(prestamo.UsuarioId, prestamo.LibroId);
        var ejemplar = reservaAsignada?.Ejemplar ?? prestamo.Libro.Ejemplares.OrderBy(ejemplar => ejemplar.Id).FirstOrDefault(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
        if (reservaAsignada is null && ejemplar is null)
        {
            throw new InvalidOperationException("No hay ejemplares disponibles para préstamo.");
        }

        var hoy = DateTime.UtcNow;
        prestamo.EjemplarId = ejemplar!.Id;
        prestamo.FechaPrestamo = hoy;
        prestamo.FechaDevolucionEsperada = hoy.AddDays(DiasPrestamo);
        prestamo.Estado = EstadoPrestamo.Activo;
        prestamo.Observaciones = $"Aprobado por personal #{adminId}.";

        if (reservaAsignada is not null)
        {
            reservaAsignada.Estado = EstadoReserva.Cumplida;
            reservaAsignada.EjemplarId = ejemplar.Id;
        }

        ejemplar.Estado = EstadoEjemplar.Prestado;
        RecalcularResumen(prestamo.Libro);
        AgregarAuditoria(adminId, "prestamo.aprobar", "Prestamo", prestamo.Id.ToString(), "Préstamo aprobado y ejemplar asignado.");
        AgregarNotificacion(prestamo.UsuarioId, TipoNotificacion.Prestamo, "Préstamo aprobado", $"Biblioteca aprobó tu préstamo y asignó el ejemplar {ejemplar.Codigo}.", $"prestamo:{prestamo.Id}");
        await context.SaveChangesAsync();
        return await ObtenerPrestamoDtoAsync(prestamo.Id);
    }

    public async Task<PrestamoDto?> DevolverAsync(int prestamoId, int usuarioId, bool esAdmin)
    {
        var prestamo = await context.Prestamos.Include(prestamo => prestamo.Libro).ThenInclude(libro => libro.Ejemplares).Include(prestamo => prestamo.Ejemplar).SingleOrDefaultAsync(prestamo => prestamo.Id == prestamoId);
        if (prestamo is null || (!esAdmin && prestamo.UsuarioId != usuarioId))
        {
            return null;
        }
        if (prestamo.Estado == EstadoPrestamo.Devuelto)
        {
            throw new InvalidOperationException("El préstamo ya fue devuelto.");
        }
        if (prestamo.Estado == EstadoPrestamo.Pendiente)
        {
            throw new InvalidOperationException("No se puede devolver una solicitud pendiente.");
        }

        prestamo.FechaDevolucionReal = DateTime.UtcNow;
        prestamo.Estado = EstadoPrestamo.Devuelto;
        await GenerarMultaSiCorrespondeAsync(prestamo, prestamo.FechaDevolucionReal.Value);
        await AsignarSiguienteReservaOLiberarEjemplarAsync(prestamo.Libro, prestamo.Ejemplar);
        AgregarAuditoria(usuarioId, "prestamo.devolver", "Prestamo", prestamo.Id.ToString(), "Devolución procesada por biblioteca.");
        AgregarNotificacion(prestamo.UsuarioId, TipoNotificacion.Prestamo, "Devolución procesada", $"Se registró la devolución de {prestamo.Libro.Titulo}.", $"prestamo:{prestamo.Id}");
        await context.SaveChangesAsync();
        return await ObtenerPrestamoDtoAsync(prestamo.Id);
    }

    public async Task<PrestamoDto?> RenovarAsync(int prestamoId, int usuarioId)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var prestamo = await context.Prestamos.SingleOrDefaultAsync(prestamo => prestamo.Id == prestamoId && prestamo.UsuarioId == usuarioId);
        if (prestamo is null)
        {
            return null;
        }
        if (prestamo.Estado != EstadoPrestamo.Activo)
        {
            throw new InvalidOperationException("Solo se pueden renovar préstamos activos.");
        }
        if (await TieneMultasPendientesAsync(usuarioId))
        {
            throw new InvalidOperationException("No puede renovar con multas pendientes.");
        }
        if (await context.Reservas.AnyAsync(reserva => reserva.LibroId == prestamo.LibroId && (reserva.Estado == EstadoReserva.Activa || reserva.Estado == EstadoReserva.Asignada)))
        {
            throw new InvalidOperationException("No puede renovar un libro con reservas pendientes.");
        }

        prestamo.FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada.AddDays(DiasPrestamo);
        AgregarAuditoria(usuarioId, "prestamo.renovar", "Prestamo", prestamo.Id.ToString(), "Préstamo renovado por lector.");
        AgregarNotificacion(prestamo.UsuarioId, TipoNotificacion.Prestamo, "Préstamo renovado", "Tu fecha de devolución fue extendida.", $"prestamo:{prestamo.Id}");
        await context.SaveChangesAsync();
        return await ObtenerPrestamoDtoAsync(prestamo.Id);
    }

    public async Task<IReadOnlyList<ReservaDto>> ListarReservasAsync(int usuarioId, bool esAdmin)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var query = ReservasConRelaciones().AsNoTracking();
        if (!esAdmin)
        {
            query = query.Where(reserva => reserva.UsuarioId == usuarioId);
        }

        return await query.OrderBy(reserva => reserva.LibroId).ThenBy(reserva => reserva.PosicionCola).Select(reserva => ReservaDto.DesdeEntidad(reserva)).ToListAsync();
    }

    public async Task<ReservaDto> CrearReservaAsync(int usuarioId, CrearReservaDto request)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var usuarioActivo = await context.Usuarios.AnyAsync(usuario => usuario.Id == usuarioId && usuario.Activo);
        var libro = await context.Libros.Include(libro => libro.Ejemplares).SingleOrDefaultAsync(libro => libro.Id == request.LibroId);
        if (!usuarioActivo)
        {
            throw new InvalidOperationException("El usuario no está activo.");
        }
        if (libro is null)
        {
            throw new ArgumentException("Libro no encontrado.");
        }
        if (libro.Ejemplares.Any(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible))
        {
            throw new InvalidOperationException("El libro tiene ejemplares disponibles; solicite préstamo directo.");
        }
        if (await context.Prestamos.AnyAsync(prestamo => prestamo.UsuarioId == usuarioId && prestamo.LibroId == request.LibroId && prestamo.Estado == EstadoPrestamo.Activo))
        {
            throw new InvalidOperationException("No puede reservar un libro que ya tiene prestado.");
        }
        if (await context.Reservas.AnyAsync(reserva => reserva.UsuarioId == usuarioId && reserva.LibroId == request.LibroId && (reserva.Estado == EstadoReserva.Activa || reserva.Estado == EstadoReserva.Asignada)))
        {
            throw new InvalidOperationException("Ya existe una reserva activa para este libro.");
        }

        var activas = await context.Reservas.CountAsync(reserva => reserva.LibroId == request.LibroId && (reserva.Estado == EstadoReserva.Activa || reserva.Estado == EstadoReserva.Asignada));
        if (activas >= MaxReservasActivasPorLibro)
        {
            throw new InvalidOperationException("El libro ya tiene 5 reservas activas.");
        }

        var posicion = await context.Reservas
            .Where(reserva => reserva.LibroId == request.LibroId && (reserva.Estado == EstadoReserva.Activa || reserva.Estado == EstadoReserva.Asignada))
            .Select(reserva => (int?)reserva.PosicionCola)
            .MaxAsync() ?? 0;
        var reservaNueva = new Reserva { UsuarioId = usuarioId, LibroId = request.LibroId, FechaReserva = DateTime.UtcNow, PosicionCola = posicion + 1, Estado = EstadoReserva.Activa };
        context.Reservas.Add(reservaNueva);
        AgregarAuditoria(usuarioId, "reserva.crear", "Reserva", "pendiente", "Reserva creada en cola FIFO.");
        AgregarNotificacion(usuarioId, TipoNotificacion.Reserva, "Reserva registrada", "Tu reserva quedó activa en la cola de espera.", $"libro:{request.LibroId}");
        await context.SaveChangesAsync();
        return (await ObtenerReservaDtoAsync(reservaNueva.Id))!;
    }

    public async Task<ReservaDto?> PrepararRetiroReservaAsync(int reservaId)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var reserva = await context.Reservas.Include(reserva => reserva.Libro).ThenInclude(libro => libro.Ejemplares).Include(reserva => reserva.Ejemplar).SingleOrDefaultAsync(reserva => reserva.Id == reservaId);
        if (reserva is null)
        {
            return null;
        }
        if (reserva.Estado != EstadoReserva.Activa)
        {
            throw new InvalidOperationException("Solo se pueden preparar reservas activas.");
        }

        var anterior = await context.Reservas.AnyAsync(otra => otra.LibroId == reserva.LibroId && otra.Estado == EstadoReserva.Activa && (otra.PosicionCola < reserva.PosicionCola || (otra.PosicionCola == reserva.PosicionCola && otra.FechaReserva < reserva.FechaReserva)));
        if (anterior)
        {
            throw new InvalidOperationException("Debe respetarse el orden FIFO de la cola.");
        }

        var ejemplar = reserva.Libro.Ejemplares.OrderBy(ejemplar => ejemplar.Id).FirstOrDefault(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
        if (ejemplar is null)
        {
            throw new InvalidOperationException("No hay ejemplares disponibles para preparar el retiro.");
        }

        reserva.Estado = EstadoReserva.Asignada;
        reserva.EjemplarId = ejemplar.Id;
        reserva.FechaExpiracion = DateTime.UtcNow.AddHours(HorasRetiroReserva);
        ejemplar.Estado = EstadoEjemplar.Reservado;
        RecalcularResumen(reserva.Libro);
        AgregarAuditoria(null, "reserva.preparar-retiro", "Reserva", reserva.Id.ToString(), "Reserva preparada para retiro con ventana de 48 horas.");
        AgregarNotificacion(reserva.UsuarioId, TipoNotificacion.Reserva, "Reserva lista para retirar", $"Ya podés retirar {reserva.Libro.Titulo}. La reserva vence el {reserva.FechaExpiracion:dd/MM/yyyy HH:mm} UTC.", $"reserva:{reserva.Id}");
        await context.SaveChangesAsync();
        return await ObtenerReservaDtoAsync(reserva.Id);
    }

    public async Task<bool> CancelarReservaAsync(int reservaId, int usuarioId, bool esAdmin)
    {
        var reserva = await context.Reservas.Include(reserva => reserva.Libro).ThenInclude(libro => libro.Ejemplares).Include(reserva => reserva.Ejemplar).SingleOrDefaultAsync(reserva => reserva.Id == reservaId);
        if (reserva is null || (!esAdmin && reserva.UsuarioId != usuarioId))
        {
            return false;
        }
        if (reserva.Estado is not EstadoReserva.Activa and not EstadoReserva.Asignada)
        {
            throw new InvalidOperationException("Solo se pueden cancelar reservas activas o asignadas.");
        }

        var estabaAsignada = reserva.Estado == EstadoReserva.Asignada;
        reserva.Estado = EstadoReserva.Cancelada;
        if (estabaAsignada)
        {
            await AsignarSiguienteReservaOLiberarEjemplarAsync(reserva.Libro, reserva.Ejemplar);
        }
        AgregarAuditoria(usuarioId, "reserva.cancelar", "Reserva", reserva.Id.ToString(), "Reserva cancelada.");
        AgregarNotificacion(reserva.UsuarioId, TipoNotificacion.Reserva, "Reserva cancelada", $"Se canceló tu reserva de {reserva.Libro.Titulo}.", $"reserva:{reserva.Id}");
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<MultaDto>> ListarMultasAsync(int usuarioId, bool esAdmin)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var query = context.Multas.Include(multa => multa.Pago).Include(multa => multa.IntentosPago).AsNoTracking().AsQueryable();
        if (!esAdmin)
        {
            query = query.Where(multa => multa.UsuarioId == usuarioId);
        }

        return await query.OrderByDescending(multa => multa.FechaGeneracion).Select(multa => MultaDto.DesdeEntidad(multa)).ToListAsync();
    }

    public async Task<IntentoPagoDto?> ProcesarPagoMultaAsync(int multaId, int usuarioId, bool esAdmin, ProcesarPagoMultaDto request)
    {
        await MarcarVencidosYExpirarReservasAsync();
        var multa = await context.Multas.Include(multa => multa.Pago).SingleOrDefaultAsync(multa => multa.Id == multaId);
        if (multa is null || (!esAdmin && multa.UsuarioId != usuarioId))
        {
            return null;
        }
        if (multa.Estado != EstadoMulta.Pendiente || multa.Pago is not null)
        {
            throw new InvalidOperationException("La multa ya fue pagada o no está pendiente.");
        }

        var ahora = DateTime.UtcNow;
        var intento = new IntentoPago
        {
            MultaId = multa.Id,
            UsuarioId = multa.UsuarioId,
            Monto = multa.Monto,
            Estado = request.Aprobar ? EstadoIntentoPago.Aprobado : EstadoIntentoPago.Rechazado,
            Referencia = $"SIM-{multa.Id}-{ahora:yyyyMMddHHmmssfff}",
            MotivoRechazo = request.Aprobar ? null : "Pago rechazado por simulación controlada.",
            FechaCreacion = ahora,
            FechaResolucion = ahora
        };
        context.IntentosPago.Add(intento);

        if (request.Aprobar)
        {
            multa.Estado = EstadoMulta.Pagada;
            multa.FechaPago = ahora;
            context.PagosMulta.Add(new PagoMulta { MultaId = multa.Id, UsuarioId = multa.UsuarioId, Monto = multa.Monto, Referencia = intento.Referencia, Recibo = $"REC-{intento.Referencia}", FechaPago = ahora });
            AgregarNotificacion(multa.UsuarioId, TipoNotificacion.Pago, "Pago simulado aprobado", $"Se registró el pago simulado de tu multa #{multa.Id}.", $"multa:{multa.Id}");
        }
        else
        {
            AgregarNotificacion(multa.UsuarioId, TipoNotificacion.Pago, "Pago simulado rechazado", $"El intento de pago de la multa #{multa.Id} fue rechazado en la simulación.", $"multa:{multa.Id}");
        }

        AgregarAuditoria(usuarioId, request.Aprobar ? "pago.aprobar" : "pago.rechazar", "Multa", multa.Id.ToString(), request.Aprobar ? "Pago simulado aprobado." : "Pago simulado rechazado.");
        await context.SaveChangesAsync();
        return IntentoPagoDto.DesdeEntidad(intento);
    }

    public async Task<IReadOnlyList<AuditEventDto>> ListarAuditoriaAsync(string? entidad, int? usuarioId)
    {
        var query = context.AuditEvents.Include(audit => audit.ActorUsuario).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entidad))
        {
            query = query.Where(audit => audit.Entidad == entidad);
        }
        if (usuarioId is not null)
        {
            query = query.Where(audit => audit.ActorUsuarioId == usuarioId);
        }

        return await query.OrderByDescending(audit => audit.Fecha).Take(100).Select(audit => AuditEventDto.DesdeEntidad(audit)).ToListAsync();
    }

    public async Task<IReadOnlyList<NotificacionDto>> ListarNotificacionesAsync(int usuarioId) => await context.Notificaciones.AsNoTracking().Where(notificacion => notificacion.UsuarioId == usuarioId).OrderByDescending(notificacion => notificacion.Fecha).Take(50).Select(notificacion => NotificacionDto.DesdeEntidad(notificacion)).ToListAsync();

    public async Task MarcarVencidosYExpirarReservasAsync()
    {
        var ahora = DateTime.UtcNow;
        var vencidos = await context.Prestamos
            .Where(prestamo => (prestamo.Estado == EstadoPrestamo.Activo || prestamo.Estado == EstadoPrestamo.Vencido) && prestamo.FechaDevolucionEsperada.Date < ahora.Date)
            .ToListAsync();
        foreach (var prestamo in vencidos)
        {
            prestamo.Estado = EstadoPrestamo.Vencido;
            await GenerarMultaSiCorrespondeAsync(prestamo, ahora);
        }

        var expiradas = await context.Reservas.Include(reserva => reserva.Libro).ThenInclude(libro => libro.Ejemplares).Include(reserva => reserva.Ejemplar).Where(reserva => reserva.Estado == EstadoReserva.Asignada && reserva.FechaExpiracion <= ahora).ToListAsync();
        foreach (var reserva in expiradas)
        {
            reserva.Estado = EstadoReserva.Expirada;
            AgregarAuditoria(null, "reserva.expirar", "Reserva", reserva.Id.ToString(), "Ventana de retiro expirada automáticamente.");
            AgregarNotificacion(reserva.UsuarioId, TipoNotificacion.Reserva, "Reserva expirada", $"La ventana de retiro de {reserva.Libro.Titulo} expiró.", $"reserva:{reserva.Id}");
            await AsignarSiguienteReservaOLiberarEjemplarAsync(reserva.Libro, reserva.Ejemplar);
        }

        if (vencidos.Count > 0 || expiradas.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }

    private IQueryable<Prestamo> PrestamosConRelaciones() => context.Prestamos.Include(prestamo => prestamo.Usuario).Include(prestamo => prestamo.Ejemplar).Include(prestamo => prestamo.Libro).ThenInclude(libro => libro.Autor).Include(prestamo => prestamo.Libro).ThenInclude(libro => libro.Categoria).Include(prestamo => prestamo.Libro).ThenInclude(libro => libro.Ejemplares).Include(prestamo => prestamo.Multa);
    private IQueryable<Reserva> ReservasConRelaciones() => context.Reservas.Include(reserva => reserva.Usuario).Include(reserva => reserva.Ejemplar).Include(reserva => reserva.Libro).ThenInclude(libro => libro.Autor).Include(reserva => reserva.Libro).ThenInclude(libro => libro.Categoria).Include(reserva => reserva.Libro).ThenInclude(libro => libro.Ejemplares);
    private async Task<PrestamoDto?> ObtenerPrestamoDtoAsync(int id) => (await PrestamosConRelaciones().AsNoTracking().SingleOrDefaultAsync(prestamo => prestamo.Id == id)) is { } prestamo ? PrestamoDto.DesdeEntidad(prestamo) : null;
    private async Task<ReservaDto?> ObtenerReservaDtoAsync(int id) => (await ReservasConRelaciones().AsNoTracking().SingleOrDefaultAsync(reserva => reserva.Id == id)) is { } reserva ? ReservaDto.DesdeEntidad(reserva) : null;
    private async Task<bool> TieneMultasPendientesAsync(int usuarioId) => await context.Multas.AnyAsync(multa => multa.UsuarioId == usuarioId && multa.Estado == EstadoMulta.Pendiente);
    private async Task<Reserva?> ObtenerReservaAsignadaAsync(int usuarioId, int libroId) => await context.Reservas.Include(reserva => reserva.Ejemplar).SingleOrDefaultAsync(reserva => reserva.UsuarioId == usuarioId && reserva.LibroId == libroId && reserva.Estado == EstadoReserva.Asignada && reserva.FechaExpiracion > DateTime.UtcNow);

    private async Task GenerarMultaSiCorrespondeAsync(Prestamo prestamo, DateTime fechaReferencia)
    {
        if (fechaReferencia.Date <= prestamo.FechaDevolucionEsperada.Date || await context.Multas.AnyAsync(multa => multa.PrestamoId == prestamo.Id))
        {
            return;
        }

        var dias = (fechaReferencia.Date - prestamo.FechaDevolucionEsperada.Date).Days;
        context.Multas.Add(new Multa { UsuarioId = prestamo.UsuarioId, PrestamoId = prestamo.Id, DiasRetraso = dias, Monto = dias * MultaPorDia, Estado = EstadoMulta.Pendiente, FechaGeneracion = DateTime.UtcNow });
        AgregarAuditoria(null, "multa.generar", "Prestamo", prestamo.Id.ToString(), $"Multa generada por {dias} días de retraso.");
        AgregarNotificacion(prestamo.UsuarioId, TipoNotificacion.Multa, "Multa generada", $"Se generó una multa por {dias} días de retraso.", $"prestamo:{prestamo.Id}");
    }

    private async Task AsignarSiguienteReservaOLiberarEjemplarAsync(Libro libro, Ejemplar? ejemplar)
    {
        var siguiente = await context.Reservas.Where(reserva => reserva.LibroId == libro.Id && reserva.Estado == EstadoReserva.Activa).OrderBy(reserva => reserva.PosicionCola).ThenBy(reserva => reserva.FechaReserva).FirstOrDefaultAsync();
        if (siguiente is null)
        {
            if (ejemplar is not null)
            {
                ejemplar.Estado = EstadoEjemplar.Disponible;
            }

            RecalcularResumen(libro);
            return;
        }

        siguiente.Estado = EstadoReserva.Asignada;
        siguiente.FechaExpiracion = DateTime.UtcNow.AddHours(HorasRetiroReserva);
        if (ejemplar is not null)
        {
            ejemplar.Estado = EstadoEjemplar.Reservado;
            siguiente.EjemplarId = ejemplar.Id;
        }

        AgregarNotificacion(siguiente.UsuarioId, TipoNotificacion.Reserva, "Reserva lista para retirar", $"Ya hay un ejemplar de {libro.Titulo} reservado para vos por 48 horas.", $"reserva:{siguiente.Id}");
        AgregarAuditoria(null, "reserva.asignar-siguiente", "Reserva", siguiente.Id.ToString(), "Reserva FIFO asignada automáticamente.");
        RecalcularResumen(libro);
    }

    private static void RecalcularResumen(Libro libro)
    {
        libro.Stock = libro.Ejemplares.Count;
        libro.Disponibles = libro.Ejemplares.Count(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
    }

    private void AgregarAuditoria(int? actorUsuarioId, string accion, string entidad, string entidadId, string resultado, string? detalle = null) => context.AuditEvents.Add(new AuditEvent { ActorUsuarioId = actorUsuarioId, Accion = accion, Entidad = entidad, EntidadId = entidadId, Resultado = resultado, Detalle = detalle });

    private void AgregarNotificacion(int usuarioId, TipoNotificacion tipo, string titulo, string mensaje, string? referencia = null) => context.Notificaciones.Add(new Notificacion { UsuarioId = usuarioId, Tipo = tipo, Titulo = titulo, Mensaje = mensaje, Referencia = referencia });

    private async Task NotificarAdminsAsync(TipoNotificacion tipo, string titulo, string mensaje, string? referencia = null)
    {
        var adminIds = await context.Usuarios
            .Where(usuario => usuario.Rol == RolUsuario.Administrador && usuario.Activo)
            .Select(usuario => usuario.Id)
            .ToListAsync();
        foreach (var adminId in adminIds)
        {
            AgregarNotificacion(adminId, tipo, titulo, mensaje, referencia);
        }
    }
}
