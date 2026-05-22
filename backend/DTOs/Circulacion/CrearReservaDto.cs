using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs.Circulacion;

public class CrearReservaDto
{
    [Required]
    public int LibroId { get; set; }
}
