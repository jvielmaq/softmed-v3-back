namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoRol
{
    /// <summary>
    /// Retorna todos los roles activos.  Los roles son compartidos (no filtran por tenant).
    /// </summary>
    public static async Task<IEnumerable<RolCore>> ObtenerTodos(MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                id_rol      AS IdRol,
                nombre_rol  AS NombreRol
            FROM TBL_ROL
            WHERE activo = 1
            ORDER BY nombre_rol";

        return await conn.QueryAsync<RolCore>(sql);
    }
}
