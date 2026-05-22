using BibliotecaAPI.Models.Entities;

namespace BibliotecaAPI.DTOs.Catalogo;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public static CategoriaDto DesdeEntidad(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre,
        Descripcion = categoria.Descripcion
    };
}
