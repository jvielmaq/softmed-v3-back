namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad INSTITUCION dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER o SM_ADM_SISTEMA.
/// </summary>
internal sealed class Institucion
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "SM_DEVELOPER", "SM_ADM_SISTEMA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaInstitucion _logica;

    public Institucion(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaInstitucion(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS"  => _logica.ObtenerTodos(),
            "OBTENER_POR_ID" => _logica.ObtenerPorId(DeserializarId()),
            "CREAR"          => _logica.Crear(
                                    Deserializar<DatosInstitucion>()
                                    ?? throw new ArgumentException("Data requerido para CREAR.")),
            "ACTUALIZAR"     => _logica.Actualizar(
                                    Deserializar<DatosInstitucion>()
                                    ?? throw new ArgumentException("Data requerido para ACTUALIZAR.")),
            "CAMBIAR_ESTADO" => EjecutarCambiarEstado(),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en INSTITUCION.")
        };
    }

    private Task<object> EjecutarCambiarEstado()
    {
        var datos = Deserializar<DatosInstitucion>()
                    ?? throw new ArgumentException("Data requerido para CAMBIAR_ESTADO.");
        return _logica.CambiarEstado(datos.IdInstitucion ?? 0, datos.Activo);
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar instituciones.");
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

        if (el.TryGetProperty("id", out var p) || el.TryGetProperty("IdInstitucion", out p))
            return p.GetInt32();

        throw new ArgumentException("Se requiere 'id' o 'IdInstitucion' en Data.");
    }
}
