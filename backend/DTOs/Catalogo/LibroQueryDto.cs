namespace BibliotecaAPI.DTOs.Catalogo;

public class LibroQueryDto
{
    public string? Search { get; set; }
    public int? AutorId { get; set; }
    public int? CategoriaId { get; set; }
    public int? Anio { get; set; }
    public bool? Disponible { get; set; }
    public string? TipoEjemplar { get; set; }
    public string? Ubicacion { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
