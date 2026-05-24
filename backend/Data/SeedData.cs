using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public static class SeedData
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        if (await context.Usuarios.AnyAsync())
        {
            return;
        }

        var usuarios = CrearUsuarios();
        var categorias = CrearCategorias();
        var autores = CrearAutores();
        var libros = CrearLibros(categorias, autores);

        context.AddRange(usuarios);
        context.AddRange(categorias);
        context.AddRange(autores);
        context.AddRange(libros);
        await context.SaveChangesAsync();

        CrearEjemplaresDesdeStock(libros);
        await context.SaveChangesAsync();

        context.AddRange(CrearPrestamosYReservas(usuarios, libros));
        await context.SaveChangesAsync();
        RecalcularResumenes(libros);
        await context.SaveChangesAsync();
    }

    private static List<Usuario> CrearUsuarios() =>
    [
        new() { Nombre = "Administrador del Sistema", Email = "admin@biblioteca.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Rol = RolUsuario.Administrador },
        new() { Nombre = "Juan Pérez", Email = "lector1@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lector123!"), Rol = RolUsuario.Lector, Telefono = "809-555-0101" },
        new() { Nombre = "María García", Email = "lector2@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lector123!"), Rol = RolUsuario.Lector, Telefono = "809-555-0102" },
        new() { Nombre = "Carlos Rodríguez", Email = "lector3@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lector123!"), Rol = RolUsuario.Lector, Telefono = "809-555-0103" }
    ];

    private static List<Categoria> CrearCategorias() =>
    [
        new() { Nombre = "Ficción", Descripcion = "Narrativa literaria y novelas." },
        new() { Nombre = "No Ficción", Descripcion = "Ensayos y textos informativos." },
        new() { Nombre = "Ciencia", Descripcion = "Divulgación y conocimiento científico." },
        new() { Nombre = "Historia", Descripcion = "Historia universal y local." },
        new() { Nombre = "Tecnología", Descripcion = "Programación e innovación." },
        new() { Nombre = "Literatura Clásica" },
        new() { Nombre = "Biografía" },
        new() { Nombre = "Autoayuda" },
        new() { Nombre = "Infantil" },
        new() { Nombre = "Académico" }
    ];

    private static List<Autor> CrearAutores() =>
    [
        new() { Nombre = "Gabriel García Márquez", Nacionalidad = "Colombiana" },
        new() { Nombre = "Mario Vargas Llosa", Nacionalidad = "Peruana" },
        new() { Nombre = "Isabel Allende", Nacionalidad = "Chilena" },
        new() { Nombre = "Julio Cortázar", Nacionalidad = "Argentina" },
        new() { Nombre = "Jorge Luis Borges", Nacionalidad = "Argentina" },
        new() { Nombre = "Pablo Neruda", Nacionalidad = "Chilena" },
        new() { Nombre = "Octavio Paz", Nacionalidad = "Mexicana" },
        new() { Nombre = "Juan Bosch", Nacionalidad = "Dominicana" },
        new() { Nombre = "Manuel del Cabral", Nacionalidad = "Dominicana" },
        new() { Nombre = "Pedro Mir", Nacionalidad = "Dominicana" },
        new() { Nombre = "Stephen Hawking", Nacionalidad = "Británica" },
        new() { Nombre = "Yuval Harari", Nacionalidad = "Israelí" },
        new() { Nombre = "Carl Sagan", Nacionalidad = "Estadounidense" },
        new() { Nombre = "Robert Martin", Nacionalidad = "Estadounidense" },
        new() { Nombre = "Andrew Hunt", Nacionalidad = "Estadounidense" }
    ];

    private static List<Libro> CrearLibros(List<Categoria> categorias, List<Autor> autores) =>
        Enumerable.Range(1, 20).Select(indice => new Libro
        {
            Titulo = $"Libro de Demostración {indice:D2}",
            Isbn = $"978-0-00-000{indice:D3}",
            Anio = 1990 + indice,
            Editorial = "Biblioteca Virtual",
            Sinopsis = "Obra de demostración con datos suficientes para probar catálogo, préstamos, reservas y reportes.",
            PortadaUrl = "https://placehold.co/300x450?text=Libro",
            Stock = indice % 5 + 1,
            Disponibles = indice <= 2 ? 0 : Math.Max(0, indice % 5 - 1),
            Categoria = categorias[(indice - 1) % categorias.Count],
            Autor = autores[(indice - 1) % autores.Count]
        }).ToList();

    private static IEnumerable<object> CrearPrestamosYReservas(List<Usuario> usuarios, List<Libro> libros)
    {
        var lector1 = usuarios[1];
        var lector2 = usuarios[2];
        var lector3 = usuarios[3];
        var hoy = DateTime.UtcNow;

        var prestamos = new List<Prestamo>
        {
            CrearPrestamo(lector1, libros[2], hoy.AddDays(-3), hoy.AddDays(11), EstadoPrestamo.Activo),
            CrearPrestamo(lector2, libros[3], hoy.AddDays(-4), hoy.AddDays(10), EstadoPrestamo.Activo),
            CrearPrestamo(lector3, libros[4], hoy.AddDays(-5), hoy.AddDays(9), EstadoPrestamo.Activo),
            CrearPrestamo(lector1, libros[5], hoy.AddDays(-6), hoy.AddDays(8), EstadoPrestamo.Activo),
            CrearPrestamo(lector2, libros[6], hoy.AddDays(-7), hoy.AddDays(7), EstadoPrestamo.Activo),
            CrearPrestamo(lector2, libros[7], hoy.AddDays(-20), hoy.AddDays(-6), EstadoPrestamo.Vencido),
            CrearPrestamo(lector3, libros[8], hoy.AddDays(-18), hoy.AddDays(-4), EstadoPrestamo.Vencido),
            CrearPrestamo(lector1, libros[9], hoy.AddDays(-25), hoy.AddDays(-11), EstadoPrestamo.Devuelto, hoy.AddDays(-1)),
            CrearPrestamo(lector2, libros[10], hoy.AddDays(-16), hoy.AddDays(-2), EstadoPrestamo.Devuelto, hoy.AddDays(-2)),
            CrearPrestamo(lector3, libros[11], hoy.AddDays(-12), hoy.AddDays(2), EstadoPrestamo.Devuelto, hoy.AddDays(-1))
        };

        return
        [
            .. prestamos,
            new Reserva { Usuario = lector1, Libro = libros[0], PosicionCola = 1, Estado = EstadoReserva.Activa },
            new Reserva { Usuario = lector2, Libro = libros[1], PosicionCola = 1, Estado = EstadoReserva.Activa }
        ];
    }

    private static void CrearEjemplaresDesdeStock(List<Libro> libros)
    {
        foreach (var libro in libros)
        {
            for (var indice = 1; indice <= libro.Stock; indice++)
            {
                libro.Ejemplares.Add(new Ejemplar
                {
                    Codigo = $"{libro.Isbn}-{indice:D3}",
                    // Todos arrancan disponibles. Los préstamos reales (CrearPrestamo)
                    // son lo único que marca un ejemplar como Prestado, así el inventario
                    // siempre coincide con la circulación.
                    Estado = EstadoEjemplar.Disponible,
                    Tipo = indice == libro.Stock && libro.Stock > 3 ? TipoEjemplar.Digital : TipoEjemplar.Fisico,
                    Ubicacion = $"Estante {((indice - 1) % 5) + 1}"
                });
            }
        }
    }

    private static Prestamo CrearPrestamo(Usuario usuario, Libro libro, DateTime fechaPrestamo, DateTime fechaEsperada, EstadoPrestamo estado, DateTime? fechaReal = null)
    {
        // Tomamos el primer ejemplar disponible. Para préstamos activos/vencidos
        // lo marcamos Prestado; para devueltos queda disponible (solo guarda historial).
        var ejemplar = libro.Ejemplares.FirstOrDefault(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible)
            ?? libro.Ejemplares.First();

        if (estado is EstadoPrestamo.Activo or EstadoPrestamo.Vencido)
        {
            ejemplar.Estado = EstadoEjemplar.Prestado;
        }

        return new Prestamo
        {
            Usuario = usuario,
            Libro = libro,
            Ejemplar = ejemplar,
            FechaPrestamo = fechaPrestamo,
            FechaDevolucionEsperada = fechaEsperada,
            FechaDevolucionReal = fechaReal,
            Estado = estado
        };
    }

    private static void RecalcularResumenes(List<Libro> libros)
    {
        foreach (var libro in libros)
        {
            libro.Stock = libro.Ejemplares.Count;
            libro.Disponibles = libro.Ejemplares.Count(ejemplar => ejemplar.Estado == EstadoEjemplar.Disponible);
        }
    }
}
