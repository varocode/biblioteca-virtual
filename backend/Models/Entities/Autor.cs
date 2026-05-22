namespace BibliotecaAPI.Models.Entities;

public class Autor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public string? Biografia { get; set; }
    public DateOnly? FechaNacimiento { get; set; }

    public ICollection<Libro> Libros { get; set; } = [];
}
