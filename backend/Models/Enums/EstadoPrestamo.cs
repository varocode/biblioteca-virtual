using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoPrestamo
{
    Pendiente = 0,
    Activo = 1,
    Devuelto = 2,
    Vencido = 3
}
