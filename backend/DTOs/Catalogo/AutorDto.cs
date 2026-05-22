using BibliotecaAPI.Models.Entities;

namespace BibliotecaAPI.DTOs.Catalogo;

public class AutorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public string? Biografia { get; set; }
    public DateOnly? FechaNacimiento { get; set; }

    public static AutorDto DesdeEntidad(Autor autor) => new()
    {
        Id = autor.Id,
        Nombre = autor.Nombre,
        Nacionalidad = autor.Nacionalidad,
        Biografia = autor.Biografia,
        FechaNacimiento = autor.FechaNacimiento
    };
}
