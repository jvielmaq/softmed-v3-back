namespace Softmed.V3.Login.Business.Modelo;

/// <summary>Payload esperado en Solicitud.Data para la acción AUTENTICAR.</summary>
public sealed class DatosLogin
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
