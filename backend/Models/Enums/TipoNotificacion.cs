using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoNotificacion
{
    Prestamo = 1,
    Reserva = 2,
    Pago = 3,
    Multa = 4
}
