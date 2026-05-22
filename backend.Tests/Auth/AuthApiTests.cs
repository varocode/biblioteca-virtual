using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BibliotecaAPI.Tests.Auth;

public class AuthApiTests
{
    [Fact]
    public async Task Registro_Login_Y_Me_Devuelven_Token_Y_Usuario_Lector()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var registro = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Nombre = "Ana Lectora",
            Email = "ana@test.com",
            Password = "Lector123!",
            Telefono = "809-555-0999"
        });

        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        var respuestaRegistro = await registro.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(respuestaRegistro);
        Assert.False(string.IsNullOrWhiteSpace(respuestaRegistro!.Token));
        Assert.Equal(RolUsuario.Lector, respuestaRegistro.Usuario.Rol);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "ana@test.com",
            Password = "Lector123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var respuestaLogin = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(respuestaLogin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", respuestaLogin!.Token);

        var me = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var usuarioActual = await me.Content.ReadFromJsonAsync<UsuarioDto>();
        Assert.Equal("ana@test.com", usuarioActual!.Email);
        Assert.Equal(RolUsuario.Lector, usuarioActual.Rol);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(respuestaLogin.Token);
        Assert.Contains(jwt.Claims, claim => claim.Type.EndsWith("/role", StringComparison.Ordinal) && claim.Value == nameof(RolUsuario.Lector));
    }

    [Fact]
    public async Task Endpoint_Protegido_Sin_Jwt_Devuelve_401()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var respuesta = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Lector_No_Puede_Listar_Usuarios_Ni_Ver_Perfil_Ajeno()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "lector1@test.com", "Lector123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var lista = await client.GetAsync("/api/usuarios");
        var perfilAjeno = await client.GetAsync("/api/usuarios/3");

        Assert.Equal(HttpStatusCode.Forbidden, lista.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, perfilAjeno.StatusCode);
    }

    [Fact]
    public async Task Administrador_Puede_Listar_Y_Actualizar_Usuarios()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var lista = await client.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.OK, lista.StatusCode);

        var actualizacion = await client.PutAsJsonAsync("/api/usuarios/2", new ActualizarUsuarioAdminDto
        {
            Nombre = "Juan Pérez Actualizado",
            Rol = RolUsuario.Lector,
            Activo = false,
            Telefono = "809-555-0101"
        });

        Assert.Equal(HttpStatusCode.OK, actualizacion.StatusCode);
        var usuario = await actualizacion.Content.ReadFromJsonAsync<UsuarioDto>();
        Assert.Equal("Juan Pérez Actualizado", usuario!.Nombre);
        Assert.False(usuario.Activo);
    }

    [Fact]
    public async Task Usuario_Puede_Actualizar_Perfil_Y_Cambiar_Password()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "lector2@test.com", "Lector123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var perfil = await client.PutAsJsonAsync("/api/usuarios/me", new ActualizarPerfilDto
        {
            Nombre = "María Perfil",
            Direccion = "Calle Demo 123"
        });
        var password = await client.PutAsJsonAsync("/api/usuarios/me/password", new CambiarPasswordDto
        {
            PasswordActual = "Lector123!",
            PasswordNuevo = "Nueva123!"
        });

        Assert.Equal(HttpStatusCode.OK, perfil.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, password.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var loginNuevo = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "lector2@test.com",
            Password = "Nueva123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginNuevo.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var respuesta = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password });
        respuesta.EnsureSuccessStatusCode();
        var auth = await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }
}

public class BibliotecaApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "clave-local-demo-cambiar-32-caracteres!!",
                ["Jwt:Issuer"] = "BibliotecaVirtual",
                ["Jwt:Audience"] = "BibliotecaVirtualUsers",
                ["Jwt:ExpirationHours"] = "8"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BibliotecaContext>>();
            services.RemoveAll<DbContextOptions>();
            foreach (var descriptor in services.Where(service => service.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal)).ToList())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<BibliotecaContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}
