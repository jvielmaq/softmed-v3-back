namespace Softmed.V3.Core.Business.Repositorio;

using Dapper;
using MySqlConnector;
using Softmed.V3.Core.Business.Modelo;

public static class RepoPersona
{
    // ─── SELECT ───────────────────────────────────────────────────────────────

    public static async Task<int> ContarTodos(int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM       TBL_PERSONA  p
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            WHERE p.activo = 1";

        return await conn.ExecuteScalarAsync<int>(sql, new { tenantId });
    }

    public static async Task<IEnumerable<PersonaCore>> ObtenerTodos(
        int tenantId, int limit, int offset, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                p.id_persona            AS IdPersona,
                p.nombres               AS Nombres,
                p.apellidos             AS Apellidos,
                p.identificador         AS Identificador,
                p.id_tipo_identificador AS IdTipoIdentificador,
                p.fecha_nacimiento      AS FechaNacimiento,
                p.id_genero             AS IdGenero,
                p.id_prevision          AS IdPrevision,
                p.url_imagen            AS UrlImagen,
                p.url_firma             AS UrlFirma,
                p.activo                AS Activo
            FROM       TBL_PERSONA  p
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            WHERE p.activo = 1
            ORDER BY p.apellidos, p.nombres
            LIMIT @limit OFFSET @offset";

        return await conn.QueryAsync<PersonaCore>(sql, new { tenantId, limit, offset });
    }

    public static async Task<PersonaCore?> ObtenerPorId(
        int idPersona, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                p.id_persona            AS IdPersona,
                p.nombres               AS Nombres,
                p.apellidos             AS Apellidos,
                p.identificador         AS Identificador,
                p.id_tipo_identificador AS IdTipoIdentificador,
                p.fecha_nacimiento      AS FechaNacimiento,
                p.id_genero             AS IdGenero,
                p.id_prevision          AS IdPrevision,
                p.url_imagen            AS UrlImagen,
                p.url_firma             AS UrlFirma,
                p.activo                AS Activo
            FROM       TBL_PERSONA  p
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            WHERE p.id_persona = @idPersona
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<PersonaCore>(
            sql, new { idPersona, tenantId });
    }

    public static async Task<IEnumerable<PersonaCore>> Buscar(
        FiltrosPersona filtros, int tenantId, MySqlConnection conn)
    {
        var sql = @"
            SELECT
                p.id_persona            AS IdPersona,
                p.nombres               AS Nombres,
                p.apellidos             AS Apellidos,
                p.identificador         AS Identificador,
                p.id_tipo_identificador AS IdTipoIdentificador,
                p.fecha_nacimiento      AS FechaNacimiento,
                p.id_genero             AS IdGenero,
                p.id_prevision          AS IdPrevision,
                p.url_imagen            AS UrlImagen,
                p.url_firma             AS UrlFirma,
                p.activo                AS Activo
            FROM       TBL_PERSONA  p
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            WHERE p.activo = 1";

        var parametros = new DynamicParameters();
        parametros.Add("tenantId", tenantId);

        if (!string.IsNullOrWhiteSpace(filtros.Identificador))
        {
            sql += " AND p.identificador LIKE @identificador";
            parametros.Add("identificador", $"%{filtros.Identificador}%");
        }

        if (!string.IsNullOrWhiteSpace(filtros.NombresApellidos))
        {
            sql += " AND CONCAT(p.nombres, ' ', p.apellidos) LIKE @nombresApellidos";
            parametros.Add("nombresApellidos", $"%{filtros.NombresApellidos}%");
        }

        sql += " ORDER BY p.apellidos, p.nombres LIMIT @limit OFFSET @offset";
        parametros.Add("limit", filtros.RegistrosPorPagina);
        parametros.Add("offset", filtros.Offset);

        return await conn.QueryAsync<PersonaCore>(sql, parametros);
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> Insertar(DatosPersona datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_PERSONA
                (nombres, apellidos, identificador, id_tipo_identificador,
                 fecha_nacimiento, id_genero, id_prevision,
                 url_imagen, url_firma, activo)
            VALUES
                (@Nombres, @Apellidos, @Identificador, @IdTipoIdentificador,
                 @FechaNacimiento, @IdGenero, @IdPrevision,
                 @UrlImagen, @UrlFirma, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task Actualizar(DatosPersona datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_PERSONA p
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            SET p.nombres             = @Nombres,
                p.apellidos           = @Apellidos,
                p.identificador       = @Identificador,
                p.id_tipo_identificador = @IdTipoIdentificador,
                p.fecha_nacimiento    = @FechaNacimiento,
                p.id_genero           = @IdGenero,
                p.id_prevision        = @IdPrevision,
                p.url_imagen          = @UrlImagen,
                p.url_firma           = @UrlFirma
            WHERE p.id_persona = @IdPersona";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombres, datos.Apellidos, datos.Identificador,
            datos.IdTipoIdentificador, datos.FechaNacimiento,
            datos.IdGenero, datos.IdPrevision,
            datos.UrlImagen, datos.UrlFirma, datos.IdPersona,
            tenantId
        });
    }

    public static async Task CambiarEstado(
        int idPersona, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_PERSONA p
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            SET p.activo = @activo
            WHERE p.id_persona = @idPersona";

        await conn.ExecuteAsync(sql, new { idPersona, activo, tenantId });
    }
}
