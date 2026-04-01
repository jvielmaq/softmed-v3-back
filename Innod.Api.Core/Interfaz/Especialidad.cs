namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad ESPECIALIDAD dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER o SM_ADM_SISTEMA.
/// </summary>
internal sealed class Especialidad
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "SM_DEVELOPER", "SM_ADM_SISTEMA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaEspecialidad _logica;

    public Especialidad(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaEspecialidad(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS" => _logica.ObtenerTodos(),
            "CREAR"         => _logica.Crear(
                                   Deserializar<DatosEspecialidad>()
                                   ?? throw new ArgumentException("Data requerido para CREAR.")),
            "ACTUALIZAR"    => _logica.Actualizar(
                                   Deserializar<DatosEspecialidad>()
                                   ?? throw new ArgumentException("Data requerido para ACTUALIZAR.")),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en ESPECIALIDAD.")
        };
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar especialidades.");
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }
}
