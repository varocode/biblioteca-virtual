using BibliotecaAPI.Data;
using BibliotecaAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliotecaAPI.Tests.Data;

public class BibliotecaContextTests
{
    [Fact]
    public void Modelo_Configura_Indices_Unicos_Principales()
    {
        using var context = CrearContexto();

        var usuario = context.Model.FindEntityType(typeof(Usuario));
        var libro = context.Model.FindEntityType(typeof(Libro));
        var categoria = context.Model.FindEntityType(typeof(Categoria));

        Assert.Contains(usuario!.GetIndexes(), index => index.IsUnique && index.Properties.Any(property => property.Name == nameof(Usuario.Email)));
        Assert.Contains(libro!.GetIndexes(), index => index.IsUnique && index.Properties.Any(property => property.Name == nameof(Libro.Isbn)));
        Assert.Contains(categoria!.GetIndexes(), index => index.IsUnique && index.Properties.Any(property => property.Name == nameof(Categoria.Nombre)));
    }

    [Fact]
    public void Modelo_Incluye_Entidades_Base_De_Demo()
    {
        using var context = CrearContexto();

        Assert.NotNull(context.Model.FindEntityType(typeof(Usuario)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Libro)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Ejemplar)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Prestamo)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Reserva)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Multa)));
    }

    private static BibliotecaContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<BibliotecaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BibliotecaContext(options);
    }
}
