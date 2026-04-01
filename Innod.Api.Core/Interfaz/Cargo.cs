namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad CARGO dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER o SM_ADM_SISTEMA.
/// </summary>
internal sealed class Cargo
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "SM_DEVELOPER", "SM_ADM_SISTEMA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaCargo _logica;

    public Cargo(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaCargo(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS" => _logica.ObtenerTodos(),
            "CREAR"         => _logica.Crear(
                                   Deserializar<DatosCargo>()
                                   ?? throw new ArgumentException("Data requerido para CREAR.")),
            "ACTUALIZAR"    => _logica.Actualizar(
                                   Deserializar<DatosCargo>()
                                   ?? throw new ArgumentException("Data requerido para ACTUALIZAR.")),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en CARGO.")
        };
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar cargos.");
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }
}
