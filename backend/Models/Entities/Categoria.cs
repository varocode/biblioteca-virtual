namespace BibliotecaAPI.Models.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Libro> Libros { get; set; } = [];
}
