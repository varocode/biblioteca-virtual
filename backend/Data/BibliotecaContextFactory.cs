using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BibliotecaAPI.Data;

public class BibliotecaContextFactory : IDesignTimeDbContextFactory<BibliotecaContext>
{
    public BibliotecaContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BibliotecaContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=biblioteca_virtual;Username=postgres;Password=postgres")
            .Options;

        return new BibliotecaContext(options);
    }
}
