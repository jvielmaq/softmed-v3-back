namespace Softmed.V3.Login;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Login.Business.Logica;

/// <summary>
/// Router del módulo LOGIN.
/// Delega cada ACTION a su clase de lógica de negocio correspondiente.
/// </summary>
public class Accion
{
    public Task<object> Ejecutar(Solicitud solicitud) =>
        solicitud.ACTION.ToUpperInvariant() switch
        {
            "AUTENTICAR" => new Autenticar(solicitud).Ejecutar(),
            _            => throw new UnauthorizedAccessException(
                                $"Acción '{solicitud.ACTION}' no reconocida en el módulo LOGIN.")
        };
}
