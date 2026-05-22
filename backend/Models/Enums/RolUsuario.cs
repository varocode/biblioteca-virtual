using System.Text.Json.Serialization;

namespace BibliotecaAPI.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RolUsuario
{
    Lector = 1,
    Administrador = 2
}
