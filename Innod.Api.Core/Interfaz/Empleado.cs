namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad EMPLEADO dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER, SM_ADM_SISTEMA o SM_ADMIN_CLINICA.
/// </summary>
internal sealed class Empleado
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_DEVELOPER", "SM_ADM_SISTEMA", "SM_ADMIN_CLINICA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaEmpleado _logica;

    public Empleado(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaEmpleado(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS"  => _logica.ObtenerTodos(),
            "CREAR"          => _logica.Crear(
                                    Deserializar<DatosEmpleado>()
                                    ?? throw new ArgumentException("Data requerido para CREAR.")),
            "CAMBIAR_ESTADO" => EjecutarCambiarEstado(),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en EMPLEADO.")
        };
    }

    private Task<object> EjecutarCambiarEstado()
    {
        var datos = Deserializar<DatosEmpleado>()
                    ?? throw new ArgumentException("Data requerido para CAMBIAR_ESTADO.");
        return _logica.CambiarEstado(datos.IdEmpleado ?? 0, datos.Activo);
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar empleados.");
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }
}
