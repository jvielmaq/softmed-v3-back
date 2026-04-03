using Dapper;
using MySqlConnector;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Business.Repositorio;

public static class RepoSedePabellon
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SEDE
    // ═══════════════════════════════════════════════════════════════════════════

    public static async Task<IEnumerable<SedeCore>> ListaSedes(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT s.id_sede        AS IdSede,
                   s.nombre         AS Nombre,
                   s.direccion      AS Direccion,
                   NULL             AS Telefono,
                   s.id_institucion AS IdInstitucion,
                   i.nombre         AS Institucion,
                   s.activo         AS Activo
            FROM       TBL_SEDE        s
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId
            ORDER BY s.nombre";

        return await conn.QueryAsync<SedeCore>(sql, new { tenantId });
    }

    public static async Task<SedeCore?> ObtieneSede(
        int idSede, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT s.id_sede        AS IdSede,
                   s.nombre         AS Nombre,
                   s.direccion      AS Direccion,
                   NULL             AS Telefono,
                   s.id_institucion AS IdInstitucion,
                   i.nombre         AS Institucion,
                   s.activo         AS Activo
            FROM       TBL_SEDE        s
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE s.id_sede   = @idSede
              AND t.id_tenant = @tenantId
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<SedeCore>(
            sql, new { idSede, tenantId });
    }

    public static async Task<int> InsertaSede(
        DatosSede datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_SEDE
                (nombre, direccion, id_institucion, activo)
            VALUES
                (@Nombre, @Direccion, @IdInstitucion, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    public static async Task ActualizaSede(
        DatosSede datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_SEDE s
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET s.nombre    = @Nombre,
                s.direccion = @Direccion
            WHERE s.id_sede   = @IdSede
              AND t.id_tenant = @tenantId";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.Direccion, datos.Telefono,
            datos.IdSede, tenantId
        });
    }

    public static async Task EliminaSede(
        int idSede, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_SEDE s
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET s.activo = 0
            WHERE s.id_sede   = @idSede
              AND t.id_tenant = @tenantId";

        await conn.ExecuteAsync(sql, new { idSede, tenantId });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PABELLON
    // ═══════════════════════════════════════════════════════════════════════════

    public static async Task<IEnumerable<PabellonCore>> ListaPabellones(
        int tenantId, MySqlConnection conn, int? idSede = null)
    {
        var whereExtra = idSede.HasValue ? " AND pb.id_sede = @idSede" : "";

        var sql = $@"
            SELECT pb.id_pabellon AS IdPabellon,
                   pb.nombre      AS Nombre,
                   pb.id_sede     AS IdSede,
                   s.nombre       AS Sede,
                   pb.activo      AS Activo
            FROM       TBL_PABELLON    pb
            INNER JOIN TBL_SEDE        s ON s.id_sede        = pb.id_sede
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId{whereExtra}
            ORDER BY s.nombre, pb.nombre";

        return await conn.QueryAsync<PabellonCore>(sql, new { tenantId, idSede });
    }

    public static async Task<int> InsertaPabellon(
        DatosPabellon datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_PABELLON (nombre, id_sede, activo)
            VALUES (@Nombre, @IdSede, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    public static async Task ActualizaPabellon(
        DatosPabellon datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_PABELLON pb
            INNER JOIN TBL_SEDE        s ON s.id_sede        = pb.id_sede
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = s.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET pb.nombre = @Nombre,
                pb.id_sede = @IdSede
            WHERE pb.id_pabellon = @IdPabellon
              AND t.id_tenant    = @tenantId";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.IdSede, datos.IdPabellon, tenantId
        });
    }
}
