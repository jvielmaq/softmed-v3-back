namespace Softmed.V3.Core.Interfaz;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;

/// <summary>
/// Router de entidad ROL dentro del módulo CORE.
/// Los roles son compartidos (no filtran por tenant).
/// </summary>
internal sealed class Rol
{
    private readonly Solicitud _solicitud;
    private readonly LogicaRol _logica;

    public Rol(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaRol();
    }

    public Task<object> Ejecutar() =>
        _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS" => _logica.ObtenerTodos(),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en ROL.")
        };
}
