using Dapper;
using MySqlConnector;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Business.Repositorio;

public static class RepoPaciente
{
    public static async Task<int> ContarPacientes(
        FiltrosPaciente f, int tenantId, MySqlConnection conn)
    {
        var sql = @"
            SELECT COUNT(DISTINCT p.id_persona)
            FROM       TBL_PERSONA      p
            INNER JOIN TBL_EXAMEN       e  ON e.id_paciente    = p.id_persona
            INNER JOIN TBL_INSTITUCION  i  ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT       t  ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId";

        var parametros = new DynamicParameters();
        parametros.Add("tenantId", tenantId);

        if (!string.IsNullOrWhiteSpace(f.Busqueda))
        {
            sql += " AND (p.identificador LIKE @busqueda OR CONCAT(p.nombres, ' ', p.apellidos) LIKE @busqueda)";
            parametros.Add("busqueda", $"%{f.Busqueda}%");
        }

        return await conn.ExecuteScalarAsync<int>(sql, parametros);
    }

    public static async Task<IEnumerable<PacienteGrilla>> ObtenerPacientes(
        FiltrosPaciente f, int tenantId, MySqlConnection conn)
    {
        var sql = @"
            SELECT
                p.id_persona        AS IdPersona,
                p.nombres           AS Nombres,
                p.apellidos         AS Apellidos,
                p.identificador     AS Identificador,
                p.fecha_nacimiento  AS FechaNacimiento,
                COUNT(e.id_examen)  AS TotalExamenes
            FROM       TBL_PERSONA      p
            INNER JOIN TBL_EXAMEN       e  ON e.id_paciente    = p.id_persona
            INNER JOIN TBL_INSTITUCION  i  ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT       t  ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId";

        var parametros = new DynamicParameters();
        parametros.Add("tenantId", tenantId);

        if (!string.IsNullOrWhiteSpace(f.Busqueda))
        {
            sql += " AND (p.identificador LIKE @busqueda OR CONCAT(p.nombres, ' ', p.apellidos) LIKE @busqueda)";
            parametros.Add("busqueda", $"%{f.Busqueda}%");
        }

        sql += @"
            GROUP BY p.id_persona, p.nombres, p.apellidos,
                     p.identificador, p.fecha_nacimiento
            ORDER BY p.apellidos, p.nombres
            LIMIT @limit OFFSET @offset";

        parametros.Add("limit", f.RegistrosPorPagina);
        parametros.Add("offset", f.Offset);

        return await conn.QueryAsync<PacienteGrilla>(sql, parametros);
    }
}
