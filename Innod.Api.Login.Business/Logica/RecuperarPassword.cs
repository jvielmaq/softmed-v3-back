namespace Softmed.V3.Login.Business.Logica;

using System.Security.Cryptography;
using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Login.Business.Repositorio;

public sealed class RecuperarPassword
{
    private readonly Solicitud _solicitud;
    public RecuperarPassword(Solicitud solicitud) => _solicitud = solicitud;

    public async Task<object> SolicitarRecuperacion()
    {
        if (_solicitud.Data is null)
            throw new ArgumentException("Data requerido.");

        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var datos = (_solicitud.Data is JsonElement el)
            ? el.Deserialize<DatosRecovery>(jsonOpts)
            : null;

        if (datos is null || string.IsNullOrWhiteSpace(datos.Email))
            throw new ArgumentException("Email requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var usuario = await RepoLogin.ObtenerUsuario(datos.Email, conn);
        // Siempre retornar OK para no revelar si el email existe
        if (usuario is null)
            return new { mensaje = "Si el correo esta registrado, recibira instrucciones." };

        // Generar token de recovery
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await RepoRecovery.InsertarToken(usuario.IdUsuario, token, _solicitud.Ip, conn);

        // TODO: Enviar email con el token via SES
        // Por ahora solo retorna el token en DEV
        var isDev = Environment.GetEnvironmentVariable("AMBIENTE") == "DEV";

        return new
        {
            mensaje = "Si el correo esta registrado, recibira instrucciones.",
            token = isDev ? token : null
        };
    }

    public async Task<object> ProcesarRecuperacion()
    {
        if (_solicitud.Data is null)
            throw new ArgumentException("Data requerido.");

        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var datos = (_solicitud.Data is JsonElement el)
            ? el.Deserialize<DatosResetPassword>(jsonOpts)
            : null;

        if (datos is null || string.IsNullOrWhiteSpace(datos.Token) || string.IsNullOrWhiteSpace(datos.NuevaPassword))
            throw new ArgumentException("Token y NuevaPassword requeridos.");

        if (datos.NuevaPassword.Length < 8)
            throw new ArgumentException("La contrasena debe tener al menos 8 caracteres.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var idUsuario = await RepoRecovery.ValidarToken(datos.Token, conn);
        if (idUsuario is null)
            throw new UnauthorizedAccessException("Token invalido o expirado.");

        var newKey = Seguridad.GeneraRandomKey();
        var newHash = Seguridad.HashPassword(datos.NuevaPassword, newKey);
        await RepoLogin.ActualizarPassword(idUsuario.Value, newHash, newKey, conn);
        await RepoRecovery.MarcarUtilizado(datos.Token, conn);

        return new { mensaje = "Contrasena actualizada correctamente." };
    }

    private sealed class DatosRecovery { public string Email { get; set; } = ""; }
    private sealed class DatosResetPassword { public string Token { get; set; } = ""; public string NuevaPassword { get; set; } = ""; }
}
