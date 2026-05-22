using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoIntentoPago
{
    Creado = 1,
    Aprobado = 2,
    Rechazado = 3
}
