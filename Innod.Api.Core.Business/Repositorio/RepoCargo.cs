namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoCargo
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<CargoCore>> ObtenerTodos(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                c.id_cargo AS IdCargo,
                c.nombre   AS Nombre,
                c.activo   AS Activo
            FROM       TBL_CARGO  c
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            WHERE c.activo = 1
            ORDER BY c.nombre";

        return await conn.QueryAsync<CargoCore>(sql, new { tenantId });
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> Insertar(DatosCargo datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_CARGO (nombre, activo)
            VALUES (@Nombre, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task Actualizar(
        DatosCargo datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_CARGO c
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            SET c.nombre = @Nombre
            WHERE c.id_cargo = @IdCargo";

        await conn.ExecuteAsync(sql, new { datos.Nombre, datos.IdCargo, tenantId });
    }
}
