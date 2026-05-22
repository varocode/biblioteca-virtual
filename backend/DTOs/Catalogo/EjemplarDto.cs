using System.Text.Json.Serialization;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;

namespace BibliotecaAPI.DTOs.Catalogo;

public class EjemplarDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EstadoEjemplar Estado { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoEjemplar Tipo { get; set; }
    public string? Ubicacion { get; set; }

    public static EjemplarDto DesdeEntidad(Ejemplar ejemplar) => new()
    {
        Id = ejemplar.Id,
        Codigo = ejemplar.Codigo,
        Estado = ejemplar.Estado,
        Tipo = ejemplar.Tipo,
        Ubicacion = ejemplar.Ubicacion
    };
}
