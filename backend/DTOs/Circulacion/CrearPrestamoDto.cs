using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Circulacion;

public class CrearPrestamoDto
{
    [Required]
    public int LibroId { get; set; }
}
