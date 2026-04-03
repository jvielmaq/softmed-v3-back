namespace Softmed.V3.Login.Business.Repositorio;

using MySqlConnector;
using Softmed.V3.Login.Business.Modelo;

public static class RepoLogin
{
    /// <summary>
    /// Obtiene el usuario por email con todos los datos necesarios para la autenticación.
    /// JOIN: TBL_USUARIO → TBL_PERSONA → TBL_ROL → TBL_INSTITUCION → TBL_TENANT
    /// </summary>
    /// <returns><see cref="UsuarioLogin"/> o null si no existe.</returns>
    public static async Task<UsuarioLogin?> ObtenerUsuario(string email, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                u.id_usuario                            AS id_usuario,
                p.nombres                               AS nombres,
                p.apellidos                             AS apellidos,
                u.email                                 AS email,
                u.clave                                 AS clave,
                u.`key`                                 AS `key`,
                r.nombre_rol                            AS nombre_rol,
                u.activo                                AS activo,
                COALESCE(u.requiere_cambio, 0)          AS requiere_cambio,
                COALESCE(i.id_institucion, 0)           AS id_institucion,
                COALESCE(t.id_tenant, 0)                AS id_tenant
            FROM       TBL_USUARIO      u
            INNER JOIN TBL_PERSONA      p  ON p.id_persona      = u.id_persona
            INNER JOIN TBL_ROL          r  ON r.id_rol           = u.id_rol
            LEFT  JOIN TBL_INSTITUCION  i  ON i.id_institucion   = u.id_institucion
            LEFT  JOIN TBL_TENANT       t  ON t.id_institucion   = i.id_institucion
            WHERE u.email = @email
            LIMIT 1";

        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new UsuarioLogin
        {
            IdUsuario     = reader.GetInt32("id_usuario"),
            Nombres       = reader.GetString("nombres"),
            Apellidos     = reader.GetString("apellidos"),
            Email         = reader.GetString("email"),
            Clave         = reader.GetString("clave"),
            Key           = reader.GetString("key"),
            NombreRol     = reader.GetString("nombre_rol"),
            Activo         = reader.GetBoolean("activo"),
            RequiereCambio = reader.GetBoolean("requiere_cambio"),
            IdInstitucion  = reader.GetInt32("id_institucion"),
            TenantId       = reader.GetInt32("id_tenant")
        };
    }

    /// <summary>Obtiene usuario por ID con todos los datos necesarios.</summary>
    public static async Task<UsuarioLogin?> ObtenerUsuarioPorId(int idUsuario, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                u.id_usuario                            AS id_usuario,
                p.nombres                               AS nombres,
                p.apellidos                             AS apellidos,
                u.email                                 AS email,
                u.clave                                 AS clave,
                u.`key`                                 AS `key`,
                r.nombre_rol                            AS nombre_rol,
                u.activo                                AS activo,
                COALESCE(u.requiere_cambio, 0)          AS requiere_cambio,
                COALESCE(i.id_institucion, 0)           AS id_institucion,
                COALESCE(t.id_tenant, 0)                AS id_tenant
            FROM       TBL_USUARIO      u
            INNER JOIN TBL_PERSONA      p  ON p.id_persona      = u.id_persona
            INNER JOIN TBL_ROL          r  ON r.id_rol           = u.id_rol
            LEFT  JOIN TBL_INSTITUCION  i  ON i.id_institucion   = u.id_institucion
            LEFT  JOIN TBL_TENANT       t  ON t.id_institucion   = i.id_institucion
            WHERE u.id_usuario = @idUsuario
            LIMIT 1";

        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new UsuarioLogin
        {
            IdUsuario      = reader.GetInt32("id_usuario"),
            Nombres        = reader.GetString("nombres"),
            Apellidos      = reader.GetString("apellidos"),
            Email          = reader.GetString("email"),
            Clave          = reader.GetString("clave"),
            Key            = reader.GetString("key"),
            NombreRol      = reader.GetString("nombre_rol"),
            Activo         = reader.GetBoolean("activo"),
            RequiereCambio = reader.GetBoolean("requiere_cambio"),
            IdInstitucion  = reader.GetInt32("id_institucion"),
            TenantId       = reader.GetInt32("id_tenant")
        };
    }

    /// <summary>Actualiza la contraseña y la key de un usuario.</summary>
    public static async Task ActualizarPassword(
        int idUsuario, string nuevaClave, string nuevaKey, MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            UPDATE TBL_USUARIO
            SET clave = @nuevaClave,
                `key` = @nuevaKey,
                requiere_cambio = 0
            WHERE id_usuario = @idUsuario";

        cmd.Parameters.AddWithValue("@nuevaClave", nuevaClave);
        cmd.Parameters.AddWithValue("@nuevaKey",   nuevaKey);
        cmd.Parameters.AddWithValue("@idUsuario",  idUsuario);

        await cmd.ExecuteNonQueryAsync();
    }
}
