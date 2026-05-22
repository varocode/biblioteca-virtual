using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoMulta
{
    Pendiente = 1,
    Pagada = 2,
    Condonada = 3
}
