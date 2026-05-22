using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Catalogo;

public class GuardarCategoriaDto
{
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}
