namespace Softmed.V3.Core.Interfaz;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Core.Business.Logica;
using Softmed.V3.Core.Business.Modelo;

/// <summary>
/// Router de entidad PERSONA dentro del módulo CORE.
/// Requiere rol SM_DEVELOPER, SM_ADM_SISTEMA, SM_ADMIN_CLINICA o SM_SOLICITANTE.
/// </summary>
internal sealed class Persona
{
    private static readonly HashSet<string> _rolesPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
            { "SM_DEVELOPER", "SM_ADM_SISTEMA", "SM_ADMIN_CLINICA", "SM_SOLICITANTE" };

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    private readonly Solicitud _solicitud;
    private readonly LogicaPersona _logica;

    public Persona(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _logica    = new LogicaPersona(solicitud);
    }

    public Task<object> Ejecutar()
    {
        ValidarRol();

        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "OBTENER_TODOS"  => _logica.ObtenerTodos(
                                    Deserializar<FiltrosPersona>() ?? new FiltrosPersona()),
            "OBTENER_POR_ID" => _logica.ObtenerPorId(DeserializarId("IdPersona")),
            "CREAR"          => _logica.Crear(
                                    Deserializar<DatosPersona>()
                                    ?? throw new ArgumentException("Data requerido para CREAR.")),
            "ACTUALIZAR"     => _logica.Actualizar(
                                    Deserializar<DatosPersona>()
                                    ?? throw new ArgumentException("Data requerido para ACTUALIZAR.")),
            "CAMBIAR_ESTADO" => EjecutarCambiarEstado(),
            "BUSCAR"         => _logica.Buscar(
                                    Deserializar<FiltrosPersona>() ?? new FiltrosPersona()),
            _ => throw new ArgumentException(
                     $"TARGET '{_solicitud.TARGET}' no reconocido en PERSONA.")
        };
    }

    private Task<object> EjecutarCambiarEstado()
    {
        var datos = Deserializar<DatosPersona>()
                    ?? throw new ArgumentException("Data requerido para CAMBIAR_ESTADO.");
        return _logica.CambiarEstado(datos.IdPersona ?? 0, datos.Activo);
    }

    private void ValidarRol()
    {
        var roles = _solicitud.UserData?.Roles ?? [];
        if (!roles.Any(r => _rolesPermitidos.Contains(r)))
            throw new UnauthorizedAccessException(
                "No tiene permisos suficientes para gestionar personas.");
    }

    private T? Deserializar<T>() where T : class
    {
        if (_solicitud.Data is not JsonElement el) return null;
        return el.Deserialize<T>(_jsonOpts);
    }

    private int DeserializarId(string campo)
    {
        if (_solicitud.Data is not JsonElement el)
            throw new ArgumentException($"Data requerido para OBTENER_POR_ID.");

        if (el.TryGetProperty("id", out var p) || el.TryGetProperty(campo, out p))
            return p.GetInt32();

        throw new ArgumentException($"Se requiere 'id' o '{campo}' en Data.");
    }
}
