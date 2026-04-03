namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoEmpleado
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<EmpleadoCore>> ObtenerTodos(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                e.id_empleado     AS IdEmpleado,
                e.id_persona      AS IdPersona,
                p.nombres         AS Nombres,
                p.apellidos       AS Apellidos,
                p.identificador   AS Identificador,
                ee.id_cargo       AS IdCargo,
                c.nombre          AS Cargo,
                ee.id_especialidad AS IdEspecialidad,
                es.nombre         AS Especialidad,
                e.activo          AS Activo
            FROM       TBL_EMPLEADO         e
            INNER JOIN TBL_PERSONA          p  ON p.id_persona       = e.id_persona
            INNER JOIN TBL_TENANT           t  ON t.id_tenant        = @tenantId
            LEFT  JOIN TBL_EMPLEADO_EMPLEOS ee ON ee.id_empleado     = e.id_empleado AND ee.activo = 1
            LEFT  JOIN TBL_CARGO            c  ON c.id_cargo         = ee.id_cargo
            LEFT  JOIN TBL_ESPECIALIDAD     es ON es.id_especialidad = ee.id_especialidad
            WHERE e.activo = 1
            ORDER BY p.apellidos, p.nombres";

        return await conn.QueryAsync<EmpleadoCore>(sql, new { tenantId });
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> Insertar(DatosEmpleado datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_EMPLEADO
                (id_persona, activo)
            VALUES
                (@IdPersona, 1);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task CambiarEstado(
        int idEmpleado, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_EMPLEADO
            SET activo = @activo
            WHERE id_empleado = @idEmpleado";

        await conn.ExecuteAsync(sql, new { idEmpleado, activo });
    }
}
