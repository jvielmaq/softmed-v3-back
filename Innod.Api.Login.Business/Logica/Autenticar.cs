namespace Softmed.V3.Login.Business.Logica;

using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Login.Business.Modelo;
using Softmed.V3.Login.Business.Repositorio;
using Softmed.V3.Login.Business.Validadores;

/// <summary>
/// Caso de uso: autenticar un usuario y retornar un JWT + datos básicos de sesión.
/// </summary>
public sealed class Autenticar
{
    private readonly Solicitud _solicitud;

    public Autenticar(Solicitud solicitud) => _solicitud = solicitud;

    public async Task<object> Ejecutar()
    {
        // ── 1. Extraer email y password de Solicitud.Data ─────────────────────
        if (_solicitud.Data is not JsonElement dataElement)
            throw new ArgumentException("El campo 'Data' es requerido para la acción AUTENTICAR.");

        var datos = dataElement.Deserialize<DatosLogin>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("No se pudo deserializar los datos de login.");

        // ── 2. Validar formato de entrada ─────────────────────────────────────
        ValidadorLogin.Validar(datos.Email, datos.Password);

        // ── 3. Consultar usuario en BD ────────────────────────────────────────
        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var usuario = await RepoLogin.ObtenerUsuario(datos.Email, conn);

        // ── 4. Verificar existencia ───────────────────────────────────────────
        if (usuario is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // ── 5. Verificar que esté activo ──────────────────────────────────────
        if (!usuario.Activo)
            throw new UnauthorizedAccessException("Usuario inactivo.");

        // ── 6. Verificar contraseña ───────────────────────────────────────────
        if (!Seguridad.VerificaPassword(datos.Password, usuario.Clave, usuario.Key))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // ── 7. Construir UserData para el JWT ─────────────────────────────────
        var userData = new UserData
        {
            IdUsuario  = usuario.IdUsuario,
            TenantId   = usuario.TenantId,
            Roles      = [usuario.NombreRol],
            Ip         = _solicitud.Ip,
            TipoOrigen = "APP"
        };

        // ── 8. Generar JWT ────────────────────────────────────────────────────
        var tokenService = new Token();
        var jwt          = tokenService.GeneraToken(userData);

        // ── 9. Retornar respuesta ─────────────────────────────────────────────
        return new
        {
            token = jwt,
            usuario = new
            {
                id       = usuario.IdUsuario,
                nombre   = usuario.Nombres,
                apellido = usuario.Apellidos,
                email    = usuario.Email,
                rol      = usuario.NombreRol
            }
        };
    }
}
