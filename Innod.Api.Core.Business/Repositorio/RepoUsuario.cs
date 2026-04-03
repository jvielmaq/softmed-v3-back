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

    // ─── ROLES ────────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> ObtenerRolesUsuario(int idUsuario, MySqlConnection conn)
    {
        const string sql = @"
            SELECT ru.id_rol_usuario AS IdRolUsuario, r.id_rol AS IdRol,
                   r.nombre_rol AS NombreRol, r.descripcion AS Descripcion, ru.activo AS Activo
            FROM TBL_ROL_USUARIO ru
            INNER JOIN TBL_ROL r ON r.id_rol = ru.id_rol
            WHERE ru.id_usuario = @idUsuario AND ru.activo = 1
            ORDER BY r.nombre_rol";
        return await conn.QueryAsync(sql, new { idUsuario });
    }

    public static async Task AgregarRol(int idUsuario, int idRol, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_ROL_USUARIO (id_usuario, id_rol, activo) VALUES (@idUsuario, @idRol, 1)
            ON DUPLICATE KEY UPDATE activo = 1";
        await conn.ExecuteAsync(sql, new { idUsuario, idRol });
    }

    public static async Task QuitarRol(int idRolUsuario, MySqlConnection conn)
    {
        await conn.ExecuteAsync("UPDATE TBL_ROL_USUARIO SET activo = 0 WHERE id_rol_usuario = @idRolUsuario",
            new { idRolUsuario });
    }

    // ─── EMPLEOS ──────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> ObtenerEmpleosUsuario(int idUsuario, MySqlConnection conn)
    {
        const string sql = @"
            SELECT ee.id_empleo AS IdEmpleo, i.nombre AS Institucion, c.nombre AS Cargo,
                   es.nombre AS Especialidad, ee.activo AS Activo
            FROM TBL_USUARIO u
            INNER JOIN TBL_EMPLEADO e ON e.id_persona = u.id_persona
            INNER JOIN TBL_EMPLEADO_EMPLEOS ee ON ee.id_empleado = e.id_empleado
            LEFT JOIN TBL_INSTITUCION i ON i.id_institucion = ee.id_institucion
            LEFT JOIN TBL_CARGO c ON c.id_cargo = ee.id_cargo
            LEFT JOIN TBL_ESPECIALIDAD es ON es.id_especialidad = ee.id_especialidad
            WHERE u.id_usuario = @idUsuario AND ee.activo = 1
            ORDER BY i.nombre";
        return await conn.QueryAsync(sql, new { idUsuario });
    }

    public static async Task<int> ObtenerOCrearEmpleado(int idPersona, MySqlConnection conn)
    {
        var id = await conn.ExecuteScalarAsync<int?>(
            "SELECT id_empleado FROM TBL_EMPLEADO WHERE id_persona = @idPersona LIMIT 1",
            new { idPersona });
        if (id.HasValue) return id.Value;
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO TBL_EMPLEADO (id_persona, activo) VALUES (@idPersona, 1); SELECT LAST_INSERT_ID();",
            new { idPersona });
    }

    public static async Task AgregarEmpleo(int idEmpleado, int idInstitucion, int? idCargo, int? idEspecialidad, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_EMPLEADO_EMPLEOS (id_empleado, id_institucion, id_cargo, id_especialidad, activo)
            VALUES (@idEmpleado, @idInstitucion, @idCargo, @idEspecialidad, 1)";
        await conn.ExecuteAsync(sql, new { idEmpleado, idInstitucion, idCargo, idEspecialidad });
    }

    public static async Task QuitarEmpleo(int idEmpleo, MySqlConnection conn)
    {
        await conn.ExecuteAsync("UPDATE TBL_EMPLEADO_EMPLEOS SET activo = 0 WHERE id_empleo = @idEmpleo",
            new { idEmpleo });
    }
}
