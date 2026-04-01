using System.Text.Json;
using MySqlConnector;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>
/// Router de entidad SEDE_PABELLON dentro del módulo SOFTMED.
/// Gestiona sedes y pabellones.
/// FeatureFlag: "examenes".
/// </summary>
internal sealed class SedePabellon
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_DEVELOPER", "SM_ADM_SISTEMA", "SM_ADMIN_CLINICA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaSedePabellon _logica;

    public SedePabellon(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaSedePabellon(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        VerificarFeatureFlag(conn);
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            // Sede
            "LISTA_SEDES"      => await _logica.ListaSedes(),
            "OBTIENE_SEDE"     => await _logica.ObtieneSede(DeserializarId("IdSede")),
            "CREAR_SEDE"       => await _logica.CrearSede(
                                      DeserializarRequerido<DatosSede>()),
            "ACTUALIZAR_SEDE"  => await _logica.ActualizarSede(
                                      DeserializarRequerido<DatosSede>()),
            "ELIMINAR_SEDE"    => await _logica.EliminarSede(DeserializarId("IdSede")),

            // Pabellon
            "LISTA_PABELLONES"    => await _logica.ListaPabellones(),
            "CREAR_PABELLON"      => await _logica.CrearPabellon(
                                        DeserializarRequerido<DatosPabellon>()),
            "ACTUALIZAR_PABELLON" => await _logica.ActualizarPabellon(
                                        DeserializarRequerido<DatosPabellon>()),

            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en SEDE_PABELLON.")
        };
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void VerificarFeatureFlag(MySqlConnection conn)
    {
        if (!FeatureFlag.IsModuleActive("examenes", _solicitud.TenantId, conn))
            throw new FeatureFlagException("examenes");
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos para gestionar sedes y pabellones.");
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
}
