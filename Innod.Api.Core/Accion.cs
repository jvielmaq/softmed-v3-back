namespace Softmed.V3.Core;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Interfaz;

/// <summary>Router principal del módulo CORE.</summary>
public class Accion
{
    public Task<object> Ejecutar(Solicitud solicitud) =>
        solicitud.ACTION.ToUpperInvariant() switch
        {
            "USUARIO"      => new Usuario(solicitud).Ejecutar(),
            "ROL"          => new Rol(solicitud).Ejecutar(),
            "PERSONA"      => new Persona(solicitud).Ejecutar(),
            "EMPLEADO"     => new Empleado(solicitud).Ejecutar(),
            "INSTITUCION"  => new Institucion(solicitud).Ejecutar(),
            "CARGO"        => new Cargo(solicitud).Ejecutar(),
            "ESPECIALIDAD" => new Especialidad(solicitud).Ejecutar(),
            _              => throw new ArgumentException(
                                  $"Acción '{solicitud.ACTION}' no reconocida en el módulo CORE.")
        };
}
