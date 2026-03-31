using System.Text.Json;
using MySqlConnector;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>
/// Router de entidad EXAMEN dentro del módulo SOFTMED.
/// Verifica FeatureFlag "examenes" antes de cada operación.
/// </summary>
internal sealed class Examen
{
    private static readonly HashSet<string> _rolesSolicitante =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_SOLICITANTE", "SM_ADM_SISTEMA", "SM_DEVELOPER" };

    private static readonly HashSet<string> _rolesJefe =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_JEFE_LABORATORIO", "SM_ADM_SISTEMA", "SM_DEVELOPER" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud    _solicitud;
    private readonly LogicaExamen _logica;

    public Examen(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaExamen(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        // Conexión sólo para el FeatureFlag; LogicaExamen abre la suya propia
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        VerificarFeatureFlag(conn);

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_GRILLA"  => await _logica.ObtenerGrilla(
                                     Deserializar<FiltrosGrilla>() ?? new FiltrosGrilla()),
            "OBTENER_POR_ID"  => await _logica.ObtenerPorId(DeserializarId()),
            "CREAR"           => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.Crear(
                                         Deserializar<DatosCrearExamen>()
                                         ?? throw new ArgumentException("Data requerido para CREAR."))),
            "CAMBIAR_ESTADO"  => await EjecutarConRol(_rolesJefe,
                                     () => _logica.CambiarEstado(
                                         Deserializar<DatosCambiarEstado>()
                                         ?? throw new ArgumentException("Data requerido para CAMBIAR_ESTADO."))),
            "OBTENER_ESTADOS" => await _logica.ObtenerEstados(),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en EXAMEN.")
        };
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void VerificarFeatureFlag(MySqlConnection conn)
    {
        if (!FeatureFlag.IsModuleActive("examenes", _solicitud.TenantId, conn))
            throw new FeatureFlagException("examenes");
    }

    private Task<object> EjecutarConRol(
        HashSet<string> rolesPermitidos, Func<Task<object>> accion)
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                $"No tiene permisos para ejecutar '{_solicitud.TARGET}' en EXAMEN.");

        return accion();
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }

    private int DeserializarId()
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException("Data requerido para OBTENER_POR_ID.");

        if (el.TryGetProperty("id", out var p) || el.TryGetProperty("IdExamen", out p))
            return p.GetInt32();

        throw new ArgumentException("Se requiere 'id' o 'IdExamen' en Data.");
    }
}
