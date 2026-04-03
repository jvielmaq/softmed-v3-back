using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>Router de entidad REPORTE dentro del módulo SOFTMED.</summary>
internal sealed class Reporte
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud    _solicitud;
    private readonly LogicaExamen _logica;

    public Reporte(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaExamen(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_DASHBOARD" => await _logica.ObtenerDashboard(
                                       Deserializar<FiltrosReporte>() ?? new FiltrosReporte()),
            "EXPORTAR"          => await _logica.ObtenerReporteExamenes(
                                       Deserializar<FiltrosReporte>() ?? new FiltrosReporte()),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en REPORTE.")
        };
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }
}
