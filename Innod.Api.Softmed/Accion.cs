using Softmed.V3.Common.Modelo;

namespace Softmed.V3.Softmed;

/// <summary>Router principal del módulo SOFTMED.</summary>
public class Accion
{
    public Task<object> Ejecutar(Solicitud solicitud) =>
        solicitud.ACTION.ToUpperInvariant() switch
        {
            "EXAMEN"            => new Interfaz.Examen(solicitud).Ejecutar(),
            "PACIENTE"          => new Interfaz.Paciente(solicitud).Ejecutar(),
            "OPERACION_INTERNA" => new Interfaz.OperacionInterna(solicitud).Ejecutar(),
            "SEDE_PABELLON"     => new Interfaz.SedePabellon(solicitud).Ejecutar(),
            "REPORTE"           => new Interfaz.Reporte(solicitud).Ejecutar(),
            "COBRO"             => new Interfaz.Cobro(solicitud).Ejecutar(),
            "LABORATORIO"       => new Interfaz.Laboratorio(solicitud).Ejecutar(),
            _                   => throw new ArgumentException(
                                       $"Acción '{solicitud.ACTION}' no reconocida en el módulo SOFTMED.")
        };
}
