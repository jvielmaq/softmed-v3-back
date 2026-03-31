using Softmed.V3.Common.Modelo;

namespace Softmed.V3;

/// <summary>
/// Router principal: delega la ejecución al módulo correspondiente según Solicitud.MODULE.
/// </summary>
public class Accion
{
    public Task<object> Ejecutar(Solicitud solicitud) =>
        solicitud.MODULE.ToUpperInvariant() switch
        {
            "LOGIN"   => new Login.Accion().Ejecutar(solicitud),
            "CORE"    => new Core.Accion().Ejecutar(solicitud),
            "SOFTMED" => new Softmed.Accion().Ejecutar(solicitud),
            _ => throw new NotSupportedException($"Módulo '{solicitud.MODULE}' no está implementado aún.")
        };
}
