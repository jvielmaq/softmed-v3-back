namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoUsuario
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<UsuarioCore>> ObtenerTodos(int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                u.id_usuario      AS IdUsuario,
                p.nombres         AS Nombres,
                p.apellidos       AS Apellidos,
                u.email           AS Email,
                r.nombre_rol      AS NombreRol,
                u.activo          AS Activo,
                u.id_institucion  AS IdInstitucion
            FROM       TBL_USUARIO     u
            INNER JOIN TBL_PERSONA     p  ON p.id_persona     = u.id_persona
            INNER JOIN TBL_ROL         r  ON r.id_rol         = u.id_rol
            INNER JOIN TBL_INSTITUCION i  ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t  ON t.id_institucion = i.id_institucion
            WHERE u.activo = 1
              AND t.id_tenant = @tenantId";

        return await conn.QueryAsync<UsuarioCore>(sql, new { tenantId });
    }

    public static async Task<UsuarioCore?> ObtenerPorId(int idUsuario, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                u.id_usuario      AS IdUsuario,
                p.nombres         AS Nombres,
                p.apellidos       AS Apellidos,
                u.email           AS Email,
                r.nombre_rol      AS NombreRol,
                u.activo          AS Activo,
                u.id_institucion  AS IdInstitucion
            FROM       TBL_USUARIO     u
            INNER JOIN TBL_PERSONA     p  ON p.id_persona     = u.id_persona
            INNER JOIN TBL_ROL         r  ON r.id_rol         = u.id_rol
            INNER JOIN TBL_INSTITUCION i  ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t  ON t.id_institucion = i.id_institucion
            WHERE u.id_usuario = @idUsuario
              AND t.id_tenant  = @tenantId
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<UsuarioCore>(sql, new { idUsuario, tenantId });
    }

    /// <summary>Devuelve la clave (hash) y key del usuario para verificación de contraseña.</summary>
    public static async Task<(string Clave, string Key)?> ObtenerCredenciales(
        int idUsuario, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT u.clave AS Clave, u.`key` AS Key
            FROM       TBL_USUARIO     u
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE u.id_usuario = @idUsuario
              AND t.id_tenant  = @tenantId
            LIMIT 1";

        var row = await conn.QueryFirstOrDefaultAsync<(string Clave, string Key)>(
                      sql, new { idUsuario, tenantId });

        return row.Clave is null ? null : row;
    }

    /// <summary>Devuelve true si ya existe un usuario con ese email (sin importar tenant).</summary>
    public static async Task<bool> ExisteEmail(string email, MySqlConnection conn)
    {
        const string sql = "SELECT COUNT(1) FROM TBL_USUARIO WHERE email = @email";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { email });
        return count > 0;
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    /// <summary>Inserta en TBL_PERSONA y retorna el id_persona generado.</summary>
    public static async Task<int> InsertarPersona(
        string nombres, string apellidos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_PERSONA (nombres, apellidos)
            VALUES (@nombres, @apellidos);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, new { nombres, apellidos });
    }

    /// <summary>Inserta en TBL_USUARIO y retorna el id_usuario generado.</summary>
    public static async Task<int> InsertarUsuario(
        int idPersona, string email, string clave, string key,
        int idRol, int idInstitucion, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_USUARIO
                (id_persona, email, clave, `key`, id_rol, id_institucion, activo)
            VALUES
                (@idPersona, @email, @clave, @key, @idRol, @idInstitucion, 1);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(
            sql, new { idPersona, email, clave, key, idRol, idInstitucion });
    }

    /// <summary>Inserta registro en TBL_EMPLEADO vinculando la persona.</summary>
    public static async Task InsertarEmpleado(int idPersona, MySqlConnection conn)
    {
        const string sql = "INSERT INTO TBL_EMPLEADO (id_persona) VALUES (@idPersona)";
        await conn.ExecuteAsync(sql, new { idPersona });
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task ActualizarPersona(
        int idPersona, string nombres, string apellidos, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_PERSONA
            SET nombres = @nombres, apellidos = @apellidos
            WHERE id_persona = @idPersona";

        await conn.ExecuteAsync(sql, new { idPersona, nombres, apellidos });
    }

    public static async Task ActualizarUsuario(
        int idUsuario, string email, int idRol, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_USUARIO u
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET u.email  = @email,
                u.id_rol = @idRol
            WHERE u.id_usuario = @idUsuario
              AND t.id_tenant  = @tenantId";

        await conn.ExecuteAsync(sql, new { idUsuario, email, idRol, tenantId });
    }

    public static async Task ActualizarClave(
        int idUsuario, string nuevaClave, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_USUARIO u
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET u.clave = @nuevaClave
            WHERE u.id_usuario = @idUsuario
              AND t.id_tenant  = @tenantId";

        await conn.ExecuteAsync(sql, new { idUsuario, nuevaClave, tenantId });
    }

    public static async Task CambiarEstado(
        int idUsuario, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_USUARIO u
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = u.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET u.activo = @activo
            WHERE u.id_usuario = @idUsuario
              AND t.id_tenant  = @tenantId";

        await conn.ExecuteAsync(sql, new { idUsuario, activo, tenantId });
    }

    /// <summary>Obtiene el id_persona de un usuario (para UPDATE de TBL_PERSONA).</summary>
    public static async Task<int> ObtenerIdPersona(int idUsuario, MySqlConnection conn)
    {
        const string sql = "SELECT id_persona FROM TBL_USUARIO WHERE id_usuario = @idUsuario LIMIT 1";
        return await conn.ExecuteScalarAsync<int>(sql, new { idUsuario });
    }
}
