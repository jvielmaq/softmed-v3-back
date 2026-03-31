namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaUsuario
{
    private readonly int _tenantId;

    public LogicaUsuario(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ─── ObtenerTodos ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var lista = await RepoUsuario.ObtenerTodos(_tenantId, conn);
        return lista;
    }

    // ─── ObtenerPorId ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerPorId(int idUsuario)
    {
        if (idUsuario <= 0)
            throw new ArgumentException("IdUsuario debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var usuario = await RepoUsuario.ObtenerPorId(idUsuario, _tenantId, conn);

        if (usuario is null)
            throw new KeyNotFoundException($"Usuario {idUsuario} no encontrado en este tenant.");

        return usuario;
    }

    // ─── Crear ────────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosUsuario datos)
    {
        ValidarDatosCrear(datos);

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        // Validar email único
        if (await RepoUsuario.ExisteEmail(datos.Email, conn))
            throw new InvalidOperationException($"El email '{datos.Email}' ya está registrado.");

        // Generar key y hashear contraseña
        var userKey = Seguridad.GeneraRandomKey();
        var clave   = Seguridad.HashPassword(datos.Password, userKey);

        // Insertar en cascada dentro de una transacción
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var idPersona  = await RepoUsuario.InsertarPersona(datos.Nombres, datos.Apellidos, conn);
            var idUsuario  = await RepoUsuario.InsertarUsuario(
                                 idPersona, datos.Email, clave, userKey,
                                 datos.IdRol, datos.IdInstitucion, conn);
            await RepoUsuario.InsertarEmpleado(idPersona, conn);

            await tx.CommitAsync();

            return new { idUsuario, mensaje = "Usuario creado correctamente." };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ─── Actualizar ───────────────────────────────────────────────────────────

    public async Task<object> Actualizar(DatosUsuario datos)
    {
        if (datos.IdUsuario is null or <= 0)
            throw new ArgumentException("IdUsuario es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombres))
            throw new ArgumentException("Nombres es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ArgumentException("Apellidos es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        // Verificar que el usuario pertenece al tenant
        var existente = await RepoUsuario.ObtenerPorId(datos.IdUsuario.Value, _tenantId, conn);
        if (existente is null)
            throw new KeyNotFoundException($"Usuario {datos.IdUsuario} no encontrado en este tenant.");

        var idPersona = await RepoUsuario.ObtenerIdPersona(datos.IdUsuario.Value, conn);

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoUsuario.ActualizarPersona(idPersona, datos.Nombres, datos.Apellidos, conn);
            await RepoUsuario.ActualizarUsuario(
                datos.IdUsuario.Value, datos.Email, datos.IdRol, _tenantId, conn);

            await tx.CommitAsync();
            return new { mensaje = "Usuario actualizado correctamente." };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ─── CambiarClave ─────────────────────────────────────────────────────────

    public async Task<object> CambiarClave(DatosUsuario datos)
    {
        if (datos.IdUsuario is null or <= 0)
            throw new ArgumentException("IdUsuario es requerido.");
        if (string.IsNullOrWhiteSpace(datos.ClaveActual))
            throw new ArgumentException("ClaveActual es requerida.");
        if (string.IsNullOrWhiteSpace(datos.ClaveNueva) || datos.ClaveNueva.Length < 6)
            throw new ArgumentException("ClaveNueva debe tener al menos 6 caracteres.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var creds = await RepoUsuario.ObtenerCredenciales(datos.IdUsuario.Value, _tenantId, conn);
        if (creds is null)
            throw new KeyNotFoundException($"Usuario {datos.IdUsuario} no encontrado en este tenant.");

        if (!Seguridad.VerificaPassword(datos.ClaveActual, creds.Value.Clave, creds.Value.Key))
            throw new UnauthorizedAccessException("La clave actual es incorrecta.");

        var nuevoHash = Seguridad.HashPassword(datos.ClaveNueva, creds.Value.Key);
        await RepoUsuario.ActualizarClave(datos.IdUsuario.Value, nuevoHash, _tenantId, conn);

        return new { mensaje = "Clave actualizada correctamente." };
    }

    // ─── CambiarEstado ────────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(int idUsuario, bool activo)
    {
        if (idUsuario <= 0)
            throw new ArgumentException("IdUsuario debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoUsuario.CambiarEstado(idUsuario, activo, _tenantId, conn);

        return new { mensaje = $"Usuario {(activo ? "activado" : "desactivado")} correctamente." };
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private static void ValidarDatosCrear(DatosUsuario datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombres))
            throw new ArgumentException("Nombres es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ArgumentException("Apellidos es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Email))
            throw new ArgumentException("Email es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Password) || datos.Password.Length < 6)
            throw new ArgumentException("Password debe tener al menos 6 caracteres.");
        if (datos.IdRol <= 0)
            throw new ArgumentException("IdRol es requerido.");
        if (datos.IdInstitucion <= 0)
            throw new ArgumentException("IdInstitucion es requerido.");
    }
}
