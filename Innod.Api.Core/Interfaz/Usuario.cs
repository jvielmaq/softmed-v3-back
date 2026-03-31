namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad USUARIO dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER o SM_ADM_SISTEMA.
/// </summary>
internal sealed class Usuario
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "SM_DEVELOPER", "SM_ADM_SISTEMA" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaUsuario _logica;

    public Usuario(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaUsuario(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        var datos = DeserializarData();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS"  => _logica.ObtenerTodos(),
            "OBTENER_POR_ID" => _logica.ObtenerPorId(datos.IdUsuario ?? 0),
            "CREAR"          => _logica.Crear(datos),
            "ACTUALIZAR"     => _logica.Actualizar(datos),
            "CAMBIAR_CLAVE"  => _logica.CambiarClave(datos),
            "CAMBIAR_ESTADO" => _logica.CambiarEstado(datos.IdUsuario ?? 0, datos.Activo),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en USUARIO.")
        };
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar usuarios.");
    }

    private DatosUsuario DeserializarData()
    {
        if (_solicitud.Data is not JsonElement el)
            return new DatosUsuario();

        return el.Deserialize<DatosUsuario>(_jsonOpts) ?? new DatosUsuario();
    }
}
