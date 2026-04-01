namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoEspecialidad
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<EspecialidadCore>> ObtenerTodos(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                e.id_especialidad AS IdEspecialidad,
                e.nombre          AS Nombre,
                e.activo          AS Activo
            FROM       TBL_ESPECIALIDAD e
            INNER JOIN TBL_TENANT       t ON t.id_tenant = @tenantId
            WHERE e.activo = 1
            ORDER BY e.nombre";

        return await conn.QueryAsync<EspecialidadCore>(sql, new { tenantId });
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> Insertar(DatosEspecialidad datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_ESPECIALIDAD (nombre, activo)
            VALUES (@Nombre, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task Actualizar(
        DatosEspecialidad datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_ESPECIALIDAD e
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            SET e.nombre = @Nombre
            WHERE e.id_especialidad = @IdEspecialidad";

        await conn.ExecuteAsync(sql, new { datos.Nombre, datos.IdEspecialidad, tenantId });
    }
}
