using System.Text.Json;
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

    private static readonly HashSet<string> _rolesEditor =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_ADM_SISTEMA", "SM_DEVELOPER" };

    // Roles que pueden cambiar estado (todos menos SM_INFORMANTE)
    private static readonly HashSet<string> _rolesCambioEstado =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_SOLICITANTE", "SM_JEFE_LABORATORIO", "SM_ADM_SISTEMA", "SM_DEVELOPER" };

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
        // FeatureFlag check con conexión que se cierra inmediatamente
        await VerificarFeatureFlagAsync();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_GRILLA"  => await _logica.ObtenerGrilla(
                                     Deserializar<FiltrosGrilla>() ?? new FiltrosGrilla()),
            "OBTENER_POR_ID"  => await _logica.ObtenerPorId(DeserializarId()),
            "CREAR"           => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.Crear(
                                         Deserializar<DatosCrearExamen>()
                                         ?? throw new ArgumentException("Data requerido para CREAR."))),
            "EDITAR"          => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.Editar(
                                         Deserializar<DatosEditarExamen>()
                                         ?? throw new ArgumentException("Data requerido para EDITAR."))),
            "FIRMAR"          => await EjecutarConRol(_rolesJefe,
                                     () => _logica.Firmar(
                                         Deserializar<DatosFirmar>()
                                         ?? throw new ArgumentException("Data requerido para FIRMAR."))),
            "EDITAR_POST_EMISION" => await EjecutarConRol(_rolesEditor,
                                     () => _logica.EditarPostEmision(
                                         Deserializar<DatosEditarPostEmision>()
                                         ?? throw new ArgumentException("Data requerido para EDITAR_POST_EMISION."))),
            "CAMBIAR_ESTADO"  => await EjecutarConRol(_rolesCambioEstado,
                                     () => _logica.CambiarEstado(
                                         Deserializar<DatosCambiarEstado>()
                                         ?? throw new ArgumentException("Data requerido para CAMBIAR_ESTADO."))),
            "OBTENER_ESTADOS" => await _logica.ObtenerEstados(DeserializarIdOpcional()),
            "GENERAR_PDF"      => await EjecutarConRol(_rolesJefe,
                                     () => _logica.GenerarPdf(DeserializarId())),
            "OBTENER_MUESTRAS" => await _logica.ObtenerMuestras(DeserializarId()),
            "AGREGAR_MUESTRA" => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.AgregarMuestra(
                                         Deserializar<DatosCrearMuestra>()
                                         ?? throw new ArgumentException("Data requerido."))),
            "ELIMINAR_MUESTRA" => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.EliminarMuestra(DeserializarId())),
            "BUSCAR_MEDICO"   => await _logica.BuscarMedicos(DeserializarTexto()),
            "OBTENER_BARCODE"  => await ObtenerBarcode(),
            "LISTAR_ADJUNTOS"  => await _logica.ListarAdjuntos(DeserializarId()),
            "AGREGAR_ADJUNTO"  => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.AgregarAdjunto(
                                         Deserializar<DatosAdjunto>()
                                         ?? throw new ArgumentException("Data requerido."))),
            "ELIMINAR_ADJUNTO" => await EjecutarConRol(_rolesSolicitante,
                                     () => _logica.EliminarAdjunto(DeserializarId())),
            "CAMBIO_MASIVO"   => await EjecutarConRol(_rolesJefe,
                                     () => _logica.CambiarEstadoMasivo(
                                         Deserializar<DatosCambioMasivo>()
                                         ?? throw new ArgumentException("Data requerido."))),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en EXAMEN.")
        };
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private async Task VerificarFeatureFlagAsync()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
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

    private async Task<object> ObtenerBarcode()
    {
        var idExamen = DeserializarId();
        var examen = await _logica.ObtenerPorId(idExamen);
        var barcode = ((dynamic)examen).examen?.barcode?.ToString() ?? "";
        return new { barcode, svg = Common.Util.BarcodeGenerator.GenerateSvg(barcode) };
    }

    private int? DeserializarIdOpcional()
    {
        if (_solicitud.Data is not JsonElement el) return null;
        if (el.TryGetProperty("id", out var p) || el.TryGetProperty("IdExamen", out p) ||
            el.TryGetProperty("ExamenId", out p))
        {
            if (p.TryGetInt32(out var val)) return val;
        }
        return null;
    }

    private string DeserializarTexto()
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException("Data requerido.");
        if (el.TryGetProperty("texto", out var p) || el.TryGetProperty("Texto", out p))
            return p.GetString() ?? "";
        throw new ArgumentException("Se requiere 'texto' en Data.");
    }
}
