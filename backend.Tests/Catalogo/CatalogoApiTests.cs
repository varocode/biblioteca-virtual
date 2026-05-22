using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Catalogo;
using BibliotecaAPI.DTOs.Common;
using BibliotecaAPI.Tests.Auth;
using Xunit;

namespace BibliotecaAPI.Tests.Catalogo;

public class CatalogoApiTests
{
    [Fact]
    public async Task Visitante_Puede_Buscar_Filtrar_Y_Paginar_Libros()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var respuesta = await client.GetAsync("/api/libros?search=Demostración&categoriaId=1&page=1&pageSize=3&sortBy=anio&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var pagina = await respuesta.Content.ReadFromJsonAsync<PagedResultDto<LibroDto>>();
        Assert.NotNull(pagina);
        Assert.Equal(1, pagina!.Page);
        Assert.Equal(3, pagina.PageSize);
        Assert.Equal(2, pagina.Total);
        Assert.All(pagina.Items, libro => Assert.Equal(1, libro.Categoria.Id));
        Assert.True(pagina.Items[0].Anio >= pagina.Items[^1].Anio);
    }

    [Fact]
    public async Task Catalogo_Filtra_Digitales_Disponibles_Y_Busca_Por_Codigo_De_Ejemplar()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = CrearLibroRequest("978-9-99-999999-9");
        request.Stock = 4;
        request.Disponibles = 4;
        var creadoResponse = await client.PostAsJsonAsync("/api/libros", request);
        creadoResponse.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = null;

        var porFormato = await client.GetAsync("/api/libros?tipoEjemplar=Digital&disponible=true&search=978-9-99-999999-9-004");

        Assert.Equal(HttpStatusCode.OK, porFormato.StatusCode);
        var pagina = await porFormato.Content.ReadFromJsonAsync<PagedResultDto<LibroDto>>();
        Assert.NotNull(pagina);
        var libro = Assert.Single(pagina!.Items);
        Assert.Contains("Digital", libro.Formatos);
        Assert.Contains("Estante 4", libro.Ubicaciones);
        Assert.Equal("4 ejemplares disponibles", libro.EtiquetaDisponibilidad);
    }

    [Fact]
    public async Task Visitante_Puede_Listar_Autores_Y_Categorias()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();

        var autores = await client.GetAsync("/api/autores");
        var categorias = await client.GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.OK, autores.StatusCode);
        Assert.Equal(HttpStatusCode.OK, categorias.StatusCode);
        Assert.NotEmpty((await autores.Content.ReadFromJsonAsync<List<AutorDto>>())!);
        Assert.NotEmpty((await categorias.Content.ReadFromJsonAsync<List<CategoriaDto>>())!);
    }

    [Fact]
    public async Task Escrituras_De_Catalogo_Requieren_Administrador()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var request = CrearLibroRequest("978-1-11-111111-1");

        var sinToken = await client.PostAsJsonAsync("/api/libros", request);

        var tokenLector = await LoginAsync(client, "lector1@test.com", "Lector123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLector);
        var lector = await client.PostAsJsonAsync("/api/libros", request);
        var inventario = await client.GetAsync("/api/ejemplares");
        var auditoria = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, sinToken.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, lector.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, inventario.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, auditoria.StatusCode);
    }

    [Fact]
    public async Task Administrador_Puede_Crear_Editar_Y_Eliminar_Catalogo()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var autorResponse = await client.PostAsJsonAsync("/api/autores", new GuardarAutorDto { Nombre = "Ada Lovelace", Nacionalidad = "Británica" });
        var categoriaResponse = await client.PostAsJsonAsync("/api/categorias", new GuardarCategoriaDto { Nombre = "Computación", Descripcion = "Libros técnicos." });
        Assert.Equal(HttpStatusCode.Created, autorResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, categoriaResponse.StatusCode);
        var autor = await autorResponse.Content.ReadFromJsonAsync<AutorDto>();
        var categoria = await categoriaResponse.Content.ReadFromJsonAsync<CategoriaDto>();

        var creadoResponse = await client.PostAsJsonAsync("/api/libros", CrearLibroRequest("978-1-11-111111-2", autor!.Id, categoria!.Id));
        Assert.Equal(HttpStatusCode.Created, creadoResponse.StatusCode);
        var creado = await creadoResponse.Content.ReadFromJsonAsync<LibroDto>();
        Assert.Equal("Arquitectura Limpia", creado!.Titulo);

        var editar = CrearLibroRequest("978-1-11-111111-2", autor.Id, categoria.Id);
        editar.Titulo = "Arquitectura Limpia Actualizada";
        var editadoResponse = await client.PutAsJsonAsync($"/api/libros/{creado.Id}", editar);
        Assert.Equal(HttpStatusCode.OK, editadoResponse.StatusCode);
        var editado = await editadoResponse.Content.ReadFromJsonAsync<LibroDto>();
        Assert.Equal("Arquitectura Limpia Actualizada", editado!.Titulo);

        var eliminarLibro = await client.DeleteAsync($"/api/libros/{creado.Id}");
        var eliminarAutor = await client.DeleteAsync($"/api/autores/{autor.Id}");
        var eliminarCategoria = await client.DeleteAsync($"/api/categorias/{categoria.Id}");

        Assert.Equal(HttpStatusCode.NoContent, eliminarLibro.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, eliminarAutor.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, eliminarCategoria.StatusCode);
    }

    [Fact]
    public async Task Libro_Rechaza_Isbn_Duplicado_Y_Disponibles_Mayor_Que_Stock()
    {
        await using var factory = new BibliotecaApiFactory();
        var client = factory.CreateClient();
        var token = await LoginAsync(client, "admin@biblioteca.com", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var duplicado = await client.PostAsJsonAsync("/api/libros", CrearLibroRequest("978-0-00-000001"));
        var stockInvalido = CrearLibroRequest("978-1-11-111111-3");
        stockInvalido.Stock = 1;
        stockInvalido.Disponibles = 2;
        var invalido = await client.PostAsJsonAsync("/api/libros", stockInvalido);

        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
    }

    private static GuardarLibroDto CrearLibroRequest(string isbn, int autorId = 1, int categoriaId = 1) => new()
    {
        Titulo = "Arquitectura Limpia",
        Isbn = isbn,
        Anio = 2024,
        Editorial = "Demo Press",
        Sinopsis = "Libro de prueba para catálogo.",
        Stock = 3,
        Disponibles = 2,
        AutorId = autorId,
        CategoriaId = categoriaId
    };

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var respuesta = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password });
        respuesta.EnsureSuccessStatusCode();
        var auth = await respuesta.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }
}
