namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoInstitucion
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<InstitucionCore>> ObtenerTodos(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                i.id_institucion AS IdInstitucion,
                i.nombre         AS Nombre,
                i.direccion      AS Direccion,
                i.telefono       AS Telefono,
                i.email          AS Email,
                i.activo         AS Activo
            FROM       TBL_INSTITUCION i
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId
            ORDER BY i.nombre";

        return await conn.QueryAsync<InstitucionCore>(sql, new { tenantId });
    }

    public static async Task<InstitucionCore?> ObtenerPorId(
        int idInstitucion, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                i.id_institucion AS IdInstitucion,
                i.nombre         AS Nombre,
                i.direccion      AS Direccion,
                i.telefono       AS Telefono,
                i.email          AS Email,
                i.activo         AS Activo
            FROM       TBL_INSTITUCION i
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE i.id_institucion = @idInstitucion
              AND t.id_tenant      = @tenantId
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<InstitucionCore>(
            sql, new { idInstitucion, tenantId });
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> Insertar(DatosInstitucion datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_INSTITUCION
                (nombre, direccion, telefono, email, activo)
            VALUES
                (@Nombre, @Direccion, @Telefono, @Email, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task Actualizar(
        DatosInstitucion datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_INSTITUCION i
            INNER JOIN TBL_TENANT  t ON t.id_institucion = i.id_institucion
            SET i.nombre    = @Nombre,
                i.direccion = @Direccion,
                i.telefono  = @Telefono,
                i.email     = @Email
            WHERE i.id_institucion = @IdInstitucion
              AND t.id_tenant      = @tenantId";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.Direccion, datos.Telefono,
            datos.Email, datos.IdInstitucion, tenantId
        });
    }

    public static async Task CambiarEstado(
        int idInstitucion, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_INSTITUCION i
            INNER JOIN TBL_TENANT  t ON t.id_institucion = i.id_institucion
            SET i.activo = @activo
            WHERE i.id_institucion = @idInstitucion
              AND t.id_tenant      = @tenantId";

        await conn.ExecuteAsync(sql, new { idInstitucion, activo, tenantId });
    }
}
