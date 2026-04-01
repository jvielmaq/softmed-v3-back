using System.Text.Json;
using MySqlConnector;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Logica;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Interfaz;

/// <summary>
/// Router de entidad PACIENTE dentro del módulo SOFTMED.
/// Verifica FeatureFlag "examenes" antes de cada operación.
/// </summary>
internal sealed class Paciente
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaPaciente _logica;

    public Paciente(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaPaciente(solicitud);
    }

    public async Task<object> Ejecutar()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        VerificarFeatureFlag(conn);

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "LISTA_PACIENTES" => await _logica.ListaPacientes(
                                     Deserializar<FiltrosPaciente>() ?? new FiltrosPaciente()),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en PACIENTE.")
        };
    }

    private void VerificarFeatureFlag(MySqlConnection conn)
    {
        if (!FeatureFlag.IsModuleActive("examenes", _solicitud.TenantId, conn))
            throw new FeatureFlagException("examenes");
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }
}
