using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>Router de entidad COBRO dentro del módulo SOFTMED. Solo Cemesi (feature flag).</summary>
internal sealed class Cobro
{
    private static readonly HashSet<string> _rolesAdmin =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_ADM_SISTEMA", "SM_DEVELOPER", "SM_JEFE_LABORATORIO" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud    _solicitud;
    private readonly LogicaExamen _logica;

    public Cobro(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaExamen(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        await VerificarFeatureFlagAsync();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "REGISTRAR" => await EjecutarConRol(
                               () => _logica.RegistrarCobro(
                                   Deserializar<DatosRegistrarCobro>()
                                   ?? throw new ArgumentException("Data requerido para REGISTRAR."))),
            "OBTENER_POR_EXAMEN" => await _logica.ObtenerCobros(DeserializarId()),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en COBRO.")
        };
    }

    private async Task VerificarFeatureFlagAsync()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        if (!FeatureFlag.IsModuleActive("cobros_fonasa", _solicitud.TenantId, conn))
            throw new FeatureFlagException("cobros_fonasa");
    }

    private Task<object> EjecutarConRol(Func<Task<object>> accion)
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesAdmin.Contains(r)))
            throw new UnauthorizedAccessException("No tiene permisos para COBRO.");
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
            throw new ArgumentException("Data requerido.");
        if (el.TryGetProperty("id", out var p) || el.TryGetProperty("ExamenId", out p))
            return p.GetInt32();
        throw new ArgumentException("Se requiere 'id' o 'ExamenId' en Data.");
    }
}
