using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Circulacion;
using BibliotecaAPI.DTOs.Dashboard;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Tests.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliotecaAPI.Tests.Circulacion;

public class CirculacionApiTests
{
    [Fact]
    public async Task Lector_Solicita_Prestamo_Y_Admin_Aprueba_Asignando_Ejemplar()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var lectorToken = await LoginAsync(client, "lector1@test.com", "Lector123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lectorToken);

        var respuesta = await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var solicitud = await respuesta.Content.ReadFromJsonAsync<PrestamoDto>();
        Assert.Equal(EstadoPrestamo.Pendiente, solicitud!.Estado);
        Assert.Null(solicitud.Ejemplar);

        var devolucionLector = await client.PostAsync($"/api/prestamos/{solicitud.Id}/devolver", null);
        Assert.Equal(HttpStatusCode.Forbidden, devolucionLector.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "admin@biblioteca.com", "Admin123!"));
        var aprobada = await (await client.PostAsync($"/api/prestamos/{solicitud.Id}/aprobar", null)).Content.ReadFromJsonAsync<PrestamoDto>();
        Assert.Equal(EstadoPrestamo.Activo, aprobada!.Estado);
        Assert.NotNull(aprobada.Ejemplar);
        Assert.Equal(EstadoEjemplar.Prestado, aprobada.Ejemplar!.Estado);

        var libro = await client.GetFromJsonAsync<LibroDto>("/api/libros/13");
        Assert.Equal(1, libro!.Disponibles);
        Assert.Equal(1, libro.EjemplaresDisponibles);
    }

    [Fact]
    public async Task Seed_Crea_Ejemplares_Y_Resumen_De_Disponibilidad_Desde_Copias()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var libro = await client.GetFromJsonAsync<LibroDto>("/api/libros/13");

        Assert.NotNull(libro);
        Assert.Equal(libro!.Stock, libro.Ejemplares.Count);
        Assert.Equal(libro.Disponibles, libro.EjemplaresDisponibles);
        Assert.All(libro.Ejemplares, ejemplar => Assert.False(string.IsNullOrWhiteSpace(ejemplar.Codigo)));
    }

    [Fact]
    public async Task Prestamo_Bloquea_Maximo_Tres_Activos()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var auth = await RegistrarAsync(client, "limite@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 12 })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 14 })).StatusCode);

        var cuarto = await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 17 });

        Assert.Equal(HttpStatusCode.Conflict, cuarto.StatusCode);
    }

    [Fact]
    public async Task Checkout_Admin_Bloquea_Usuario_Inactivo_Multas_Pendientes_Y_Sin_Disponibilidad()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector3@test.com", "Lector123!"));
        await DesactivarUsuarioAsync(factory, "lector3@test.com");
        var usuarioInactivo = await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector2@test.com", "Lector123!"));
        var multaPendiente = await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var solicitud = await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 1 });
        var prestamo = await solicitud.Content.ReadFromJsonAsync<PrestamoDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "admin@biblioteca.com", "Admin123!"));
        var sinDisponibilidad = await client.PostAsync($"/api/prestamos/{prestamo!.Id}/aprobar", null);

        Assert.Equal(HttpStatusCode.Conflict, usuarioInactivo.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, multaPendiente.StatusCode);
        Assert.Equal(HttpStatusCode.Created, solicitud.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, sinDisponibilidad.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        Assert.DoesNotContain(await context.Ejemplares.Where(ejemplar => ejemplar.LibroId == 1).ToListAsync(), ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
    }

    [Fact]
    public async Task Devolucion_Vencida_Genera_Multa_Y_Asigna_Reserva_FIFO_48h()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var adminToken = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var autor = await (await client.PostAsJsonAsync("/api/autores", new GuardarAutorDto { Nombre = "Autor Circulación" })).Content.ReadFromJsonAsync<AutorDto>();
        var categoria = await (await client.PostAsJsonAsync("/api/categorias", new GuardarCategoriaDto { Nombre = "Circulación" })).Content.ReadFromJsonAsync<CategoriaDto>();
        var libro = await (await client.PostAsJsonAsync("/api/libros", new GuardarLibroDto { Titulo = "Libro FIFO", Isbn = "978-9-99-999999-1", Anio = 2026, Stock = 1, Disponibles = 1, AutorId = autor!.Id, CategoriaId = categoria!.Id })).Content.ReadFromJsonAsync<LibroDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var solicitud = await (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = libro!.Id })).Content.ReadFromJsonAsync<PrestamoDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var prestamo = await (await client.PostAsync($"/api/prestamos/{solicitud!.Id}/aprobar", null)).Content.ReadFromJsonAsync<PrestamoDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector3@test.com", "Lector123!"));
        var reservaResponse = await client.PostAsJsonAsync("/api/reservas", new CrearReservaDto { LibroId = libro.Id });
        Assert.Equal(HttpStatusCode.Created, reservaResponse.StatusCode);

        await VencerPrestamoAsync(factory, prestamo!.Id);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var devolucion = await client.PostAsync($"/api/prestamos/{prestamo.Id}/devolver", null);
        Assert.Equal(HttpStatusCode.OK, devolucion.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector3@test.com", "Lector123!"));
        var reservas = await client.GetFromJsonAsync<List<ReservaDto>>("/api/reservas");
        var asignada = Assert.Single(reservas!, reserva => reserva.Libro.Id == libro.Id);
        Assert.Equal(EstadoReserva.Asignada, asignada.Estado);
        Assert.True(asignada.FechaExpiracion > DateTime.UtcNow.AddHours(47));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var multas = await client.GetFromJsonAsync<List<MultaDto>>("/api/multas");
        Assert.Contains(multas!, multa => multa.PrestamoId == prestamo.Id && multa.Monto == 150m);
    }

    [Fact]
    public async Task Admin_Prepara_Reserva_Con_Ventana_48h_Y_Expiracion_Libera_Ejemplar()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "admin@biblioteca.com", "Admin123!"));
        var autor = await (await client.PostAsJsonAsync("/api/autores", new GuardarAutorDto { Nombre = "Autor Retiro" })).Content.ReadFromJsonAsync<AutorDto>();
        var categoria = await (await client.PostAsJsonAsync("/api/categorias", new GuardarCategoriaDto { Nombre = "Retiro" })).Content.ReadFromJsonAsync<CategoriaDto>();
        var libro = await (await client.PostAsJsonAsync("/api/libros", new GuardarLibroDto { Titulo = "Libro Retiro", Isbn = "978-9-99-999999-2", Anio = 2026, Stock = 1, Disponibles = 1, AutorId = autor!.Id, CategoriaId = categoria!.Id })).Content.ReadFromJsonAsync<LibroDto>();
        var reservaId = await CrearReservaActivaAsync(factory, libro!.Id, "lector3@test.com");

        var preparada = await (await client.PostAsync($"/api/reservas/{reservaId}/preparar-retiro", null)).Content.ReadFromJsonAsync<ReservaDto>();

        Assert.Equal(EstadoReserva.Asignada, preparada!.Estado);
        Assert.NotNull(preparada.Ejemplar);
        Assert.True(preparada.FechaExpiracion > DateTime.UtcNow.AddHours(47));

        await ExpirarReservaAsync(factory, reservaId);
        var reservas = await client.GetFromJsonAsync<List<ReservaDto>>("/api/reservas");
        var expirada = Assert.Single(reservas!, reserva => reserva.Id == reservaId);
        Assert.Equal(EstadoReserva.Expirada, expirada.Estado);

        var copias = await client.GetFromJsonAsync<List<EjemplarAdminDto>>("/api/ejemplares?libroId=" + libro.Id);
        Assert.Equal(EstadoEjemplar.Disponible, copias!.Single().Detalle.Estado);
    }

    [Fact]
    public async Task Lector_No_Puede_Usar_Endpoints_Admin_De_Circulacion()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var solicitud = await (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 })).Content.ReadFromJsonAsync<PrestamoDto>();

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/prestamos/{solicitud!.Id}/aprobar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/prestamos/{solicitud.Id}/devolver", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/reservas/1/preparar-retiro", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/ejemplares")).StatusCode);
    }

    [Fact]
    public async Task Pago_Simulado_Registra_Rechazo_Aprobacion_Recibo_Y_Bloquea_Doble_Pago()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var adminToken = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var autor = await (await client.PostAsJsonAsync("/api/autores", new GuardarAutorDto { Nombre = "Autor Pagos" })).Content.ReadFromJsonAsync<AutorDto>();
        var categoria = await (await client.PostAsJsonAsync("/api/categorias", new GuardarCategoriaDto { Nombre = "Pagos" })).Content.ReadFromJsonAsync<CategoriaDto>();
        var libro = await (await client.PostAsJsonAsync("/api/libros", new GuardarLibroDto { Titulo = "Libro Pago", Isbn = "978-9-99-999999-3", Anio = 2026, Stock = 1, Disponibles = 1, AutorId = autor!.Id, CategoriaId = categoria!.Id })).Content.ReadFromJsonAsync<LibroDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var solicitud = await (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = libro!.Id })).Content.ReadFromJsonAsync<PrestamoDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var prestamo = await (await client.PostAsync($"/api/prestamos/{solicitud!.Id}/aprobar", null)).Content.ReadFromJsonAsync<PrestamoDto>();
        await VencerPrestamoAsync(factory, prestamo!.Id);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/prestamos/{prestamo.Id}/devolver", null)).StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var multa = (await client.GetFromJsonAsync<List<MultaDto>>("/api/multas"))!.Single(multa => multa.PrestamoId == prestamo.Id);
        var rechazo = await (await client.PostAsJsonAsync($"/api/multas/{multa.Id}/pagar", new ProcesarPagoMultaDto { Aprobar = false })).Content.ReadFromJsonAsync<IntentoPagoDto>();
        Assert.Equal(EstadoIntentoPago.Rechazado, rechazo!.Estado);

        var aprobado = await (await client.PostAsJsonAsync($"/api/multas/{multa.Id}/pagar", new ProcesarPagoMultaDto { Aprobar = true })).Content.ReadFromJsonAsync<IntentoPagoDto>();
        Assert.Equal(EstadoIntentoPago.Aprobado, aprobado!.Estado);
        var doblePago = await client.PostAsJsonAsync($"/api/multas/{multa.Id}/pagar", new ProcesarPagoMultaDto { Aprobar = true });
        Assert.Equal(HttpStatusCode.Conflict, doblePago.StatusCode);

        var actualizada = (await client.GetFromJsonAsync<List<MultaDto>>("/api/multas"))!.Single(item => item.Id == multa.Id);
        Assert.Equal(EstadoMulta.Pagada, actualizada.Estado);
        Assert.NotNull(actualizada.Pago);
        Assert.StartsWith("REC-SIM-", actualizada.Pago!.Recibo);
        Assert.Equal(2, actualizada.IntentosPago.Count);
    }

    [Fact]
    public async Task Auditoria_Y_Notificaciones_Son_Consultables_Y_Auditoria_No_Se_Muta()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var solicitud = await (await client.PostAsJsonAsync("/api/prestamos", new CrearPrestamoDto { LibroId = 13 })).Content.ReadFromJsonAsync<PrestamoDto>();
        var notificaciones = await client.GetFromJsonAsync<List<NotificacionDto>>("/api/notificaciones");
        Assert.Contains(notificaciones!, notificacion => notificacion.Tipo == TipoNotificacion.Prestamo);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/audit")).StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "admin@biblioteca.com", "Admin123!"));
        await client.PostAsync($"/api/prestamos/{solicitud!.Id}/aprobar", null);
        var audit = await client.GetFromJsonAsync<List<AuditEventDto>>("/api/audit");
        Assert.Contains(audit!, evento => evento.Accion == "prestamo.aprobar");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync($"/api/audit/{audit.First().Id}")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        var evento = await context.AuditEvents.FirstAsync();
        evento.Resultado = "mutado";
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Dashboard_Es_Solo_Admin_Y_Calcula_Metricas()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "lector1@test.com", "Lector123!"));
        var lector = await client.GetAsync("/api/dashboard/resumen");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "admin@biblioteca.com", "Admin123!"));
        var admin = await client.GetAsync("/api/dashboard/resumen");
        var resumen = await admin.Content.ReadFromJsonAsync<DashboardResumenDto>();

        Assert.Equal(HttpStatusCode.Forbidden, lector.StatusCode);
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);
        Assert.True(resumen!.TotalLibros >= 20);
        Assert.True(resumen.PrestamosVencidos >= 2);
        Assert.NotEmpty(resumen.TopLibros);
        Assert.NotEmpty(resumen.PrestamosPorMes);
        Assert.True(resumen.MontoMultasPendientes >= 500m);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var respuesta = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password });
        respuesta.EnsureSuccessStatusCode();
        var auth = await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    private static async Task<AuthResponseDto> RegistrarAsync(HttpClient client, string email)
    {
        var respuesta = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto { Nombre = "Usuario Límite", Email = email, Password = "Lector123!" });
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    private static async Task VencerPrestamoAsync(BibliotecaApiFactory factory, int prestamoId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        var prestamo = await context.Prestamos.FindAsync(prestamoId);
        prestamo!.FechaDevolucionEsperada = DateTime.UtcNow.AddDays(-3);
        await context.SaveChangesAsync();
    }

    private static async Task DesactivarUsuarioAsync(BibliotecaApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        var usuario = await context.Usuarios.SingleAsync(usuario => usuario.Email == email);
        usuario.Activo = false;
        await context.SaveChangesAsync();
    }

    private static async Task<int> CrearReservaActivaAsync(BibliotecaApiFactory factory, int libroId, string email)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        var usuario = await context.Usuarios.SingleAsync(usuario => usuario.Email == email);
        var reserva = new Reserva { UsuarioId = usuario.Id, LibroId = libroId, FechaReserva = DateTime.UtcNow, PosicionCola = 1, Estado = EstadoReserva.Activa };
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();
        return reserva.Id;
    }

    private static async Task ExpirarReservaAsync(BibliotecaApiFactory factory, int reservaId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        var reserva = await context.Reservas.FindAsync(reservaId);
        reserva!.FechaExpiracion = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();
    }

    private sealed class EjemplarAdminDto
    {
        public int Id { get; set; }
        public int LibroId { get; set; }
        public string LibroTitulo { get; set; } = string.Empty;
        public EjemplarDto Detalle { get; set; } = new();
    }
}
