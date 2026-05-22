using System.Net;
using System.Net.Http.Json;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Common;
using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.Tests.Auth;
using Xunit;

namespace BibliotecaAPI.Tests.Foundation;

public class DemoStartupEvidenceTests
{
    [Fact]
    public async Task Demo_Arranque_Expone_Health_Swagger_Credenciales_Y_Catalogo_Seed()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var health = await client.GetAsync("/health");
        var swagger = await client.GetAsync("/swagger/index.html");
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "admin@biblioteca.com",
            Password = "Admin123!"
        });
        var catalogo = await client.GetAsync("/api/libros?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, catalogo.StatusCode);

        var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        var libros = await catalogo.Content.ReadFromJsonAsync<PagedResultDto<LibroDto>>();

        Assert.False(string.IsNullOrWhiteSpace(auth?.Token));
        Assert.Equal("admin@biblioteca.com", auth!.Usuario.Email);
        Assert.Equal("Administrador", auth.Usuario.Rol.ToString());
        Assert.True(libros!.Total >= 20);
        Assert.NotEmpty(libros.Items);
    }
}
