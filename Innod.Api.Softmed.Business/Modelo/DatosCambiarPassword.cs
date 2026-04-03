namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload para cambiar contraseña.</summary>
public sealed class DatosCambiarPassword
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNuevo  { get; set; } = string.Empty;
}
