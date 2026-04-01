using System.Text.Json;
using MySqlConnector;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>
/// Router de entidad OPERACION_INTERNA dentro del módulo SOFTMED.
/// Gestiona tipo_muestra, organo y tipo_solicitud.
/// FeatureFlag: "productos".
/// </summary>
internal sealed class OperacionInterna
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_DEVELOPER", "SM_ADM_SISTEMA", "SM_ADMIN_CLINICA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaOperacionInterna _logica;

    public OperacionInterna(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaOperacionInterna(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        VerificarFeatureFlag(conn);
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            // Tipo Muestra
            "TIPO_MUESTRA_LISTA"       => await _logica.TipoMuestraLista(),
            "TIPO_MUESTRA_OBTIENE"     => await _logica.TipoMuestraObtiene(DeserializarId("id")),
            "TIPO_MUESTRA_NUEVO"       => await _logica.TipoMuestraNuevo(
                                              DeserializarRequerido<DatosTipoMuestra>()),
            "TIPO_MUESTRA_EDITAR"      => await _logica.TipoMuestraEditar(
                                              DeserializarRequerido<DatosTipoMuestra>()),
            "TIPO_MUESTRA_CAMBIAESTADO"=> await EjecutarCambiaEstado(
                                              (id, a) => _logica.TipoMuestraCambiaEstado(id, a)),

            // Organo
            "ORGANO_LISTA"             => await _logica.OrganoLista(),
            "ORGANO_OBTIENE"           => await _logica.OrganoObtiene(DeserializarId("id")),
            "ORGANO_NUEVO"             => await _logica.OrganoNuevo(
                                              DeserializarRequerido<DatosOrgano>()),
            "ORGANO_EDITAR"            => await _logica.OrganoEditar(
                                              DeserializarRequerido<DatosOrgano>()),
            "ORGANO_CAMBIAESTADO"      => await EjecutarCambiaEstado(
                                              (id, a) => _logica.OrganoCambiaEstado(id, a)),

            // Tipo Solicitud
            "TIPO_SOLICITUD_LISTA"     => await _logica.TipoSolicitudLista(),
            "TIPO_SOLICITUD_OBTIENE"   => await _logica.TipoSolicitudObtiene(DeserializarId("id")),
            "TIPO_SOLICITUD_NUEVO"     => await _logica.TipoSolicitudNuevo(
                                              DeserializarRequerido<DatosTipoSolicitud>()),
            "TIPO_SOLICITUD_EDITAR"    => await _logica.TipoSolicitudEditar(
                                              DeserializarRequerido<DatosTipoSolicitud>()),
            "TIPO_SOLICITUD_CAMBIAESTADO" => await EjecutarCambiaEstado(
                                              (id, a) => _logica.TipoSolicitudCambiaEstado(id, a)),

            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en OPERACION_INTERNA.")
        };
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void VerificarFeatureFlag(MySqlConnection conn)
    {
        if (!FeatureFlag.IsModuleActive("productos", _solicitud.TenantId, conn))
            throw new FeatureFlagException("productos");
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos para ejecutar operaciones internas.");
    }

    private T DeserializarRequerido<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException($"Data es requerido para '{_solicitud.TARGET}'.");
        return el.Deserialize<T>(_jsonOpts)
               ?? throw new ArgumentException($"Data es requerido para '{_solicitud.TARGET}'.");
    }

    private int DeserializarId(string campo)
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException("Data requerido.");
        if (el.TryGetProperty("id", out var p) || el.TryGetProperty(campo, out p))
            return p.GetInt32();
        throw new ArgumentException($"Se requiere 'id' o '{campo}' en Data.");
    }

    private Task<object> EjecutarCambiaEstado(Func<int, bool, Task<object>> accion)
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException("Data requerido para cambiar estado.");

        var id = 0;
        var activo = true;

        if (el.TryGetProperty("id", out var pId))
            id = pId.GetInt32();
        if (el.TryGetProperty("activo", out var pActivo) ||
            el.TryGetProperty("Activo", out pActivo))
            activo = pActivo.GetBoolean();

        return accion(id, activo);
    }
}
