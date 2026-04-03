namespace Softmed.V3.Login.Business.Logica;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Login.Business.Repositorio;

/// <summary>Caso de uso: cambiar la contraseña del usuario autenticado.</summary>
public sealed class CambiarPassword
{
    private readonly Solicitud _solicitud;

    public CambiarPassword(Solicitud solicitud) => _solicitud = solicitud;

    public async Task<object> Ejecutar()
    {
        if (_solicitud.Data is null)
            throw new ArgumentException("El campo 'Data' es requerido.");

        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        DatosCambiarPwd? datos;

        if (_solicitud.Data is JsonElement el)
            datos = el.Deserialize<DatosCambiarPwd>(jsonOpts);
        else
            datos = JsonSerializer.Deserialize<DatosCambiarPwd>(_solicitud.Data as string ?? "", jsonOpts);

        if (datos is null)
            throw new ArgumentException("No se pudo deserializar los datos.");

        if (string.IsNullOrWhiteSpace(datos.PasswordActual))
            throw new ArgumentException("PasswordActual es requerido.");
        if (string.IsNullOrWhiteSpace(datos.PasswordNuevo))
            throw new ArgumentException("PasswordNuevo es requerido.");
        if (datos.PasswordNuevo.Length < 8)
            throw new ArgumentException("La nueva contraseña debe tener al menos 8 caracteres.");

        var idUsuario = _solicitud.UserData?.IdUsuario
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        // Obtener clave y key actuales
        var usuario = await RepoLogin.ObtenerUsuarioPorId(idUsuario, conn);
        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        // Verificar contraseña actual
        if (!Seguridad.VerificaPassword(datos.PasswordActual, usuario.Clave, usuario.Key))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        // Generar nuevo hash con nueva key
        var newKey  = Seguridad.GeneraRandomKey();
        var newHash = Seguridad.HashPassword(datos.PasswordNuevo, newKey);

        await RepoLogin.ActualizarPassword(idUsuario, newHash, newKey, conn);

        return new { mensaje = "Contraseña actualizada correctamente." };
    }

    private sealed class DatosCambiarPwd
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNuevo  { get; set; } = string.Empty;
    }
}
