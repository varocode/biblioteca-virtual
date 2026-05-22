using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoReserva
{
    Activa = 1,
    Asignada = 2,
    Cumplida = 3,
    Cancelada = 4,
    Expirada = 5
}
