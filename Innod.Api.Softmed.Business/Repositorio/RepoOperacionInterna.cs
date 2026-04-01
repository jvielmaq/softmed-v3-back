using Dapper;
using MySqlConnector;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Business.Repositorio;

public static class RepoOperacionInterna
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TIPO MUESTRA
    // ═══════════════════════════════════════════════════════════════════════════

    public static async Task<IEnumerable<TipoMuestraCore>> TipoMuestraLista(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT tm.id_tipo_muestra AS IdTipoMuestra,
                   tm.nombre          AS Nombre,
                   tm.descripcion     AS Descripcion,
                   tm.activo          AS Activo
            FROM       TBL_TIPO_MUESTRA tm
            INNER JOIN TBL_TENANT       t ON t.id_tenant = @tenantId
            ORDER BY tm.nombre";

        return await conn.QueryAsync<TipoMuestraCore>(sql, new { tenantId });
    }

    public static async Task<TipoMuestraCore?> TipoMuestraObtiene(
        int id, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT tm.id_tipo_muestra AS IdTipoMuestra,
                   tm.nombre          AS Nombre,
                   tm.descripcion     AS Descripcion,
                   tm.activo          AS Activo
            FROM       TBL_TIPO_MUESTRA tm
            INNER JOIN TBL_TENANT       t ON t.id_tenant = @tenantId
            WHERE tm.id_tipo_muestra = @id
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<TipoMuestraCore>(
            sql, new { id, tenantId });
    }

    public static async Task<int> TipoMuestraInserta(
        DatosTipoMuestra datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_TIPO_MUESTRA (nombre, descripcion, activo)
            VALUES (@Nombre, @Descripcion, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    public static async Task TipoMuestraActualiza(
        DatosTipoMuestra datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_TIPO_MUESTRA tm
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            SET tm.nombre      = @Nombre,
                tm.descripcion = @Descripcion
            WHERE tm.id_tipo_muestra = @IdTipoMuestra";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.Descripcion, datos.IdTipoMuestra, tenantId
        });
    }

    public static async Task TipoMuestraCambiaEstado(
        int id, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_TIPO_MUESTRA tm
            INNER JOIN TBL_TENANT   t ON t.id_tenant = @tenantId
            SET tm.activo = @activo
            WHERE tm.id_tipo_muestra = @id";

        await conn.ExecuteAsync(sql, new { id, activo, tenantId });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ORGANO
    // ═══════════════════════════════════════════════════════════════════════════

    public static async Task<IEnumerable<OrganoCore>> OrganoLista(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT o.id_organo   AS IdOrgano,
                   o.nombre      AS Nombre,
                   o.descripcion AS Descripcion,
                   o.activo      AS Activo
            FROM       TBL_ORGANO o
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            ORDER BY o.nombre";

        return await conn.QueryAsync<OrganoCore>(sql, new { tenantId });
    }

    public static async Task<OrganoCore?> OrganoObtiene(
        int id, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT o.id_organo   AS IdOrgano,
                   o.nombre      AS Nombre,
                   o.descripcion AS Descripcion,
                   o.activo      AS Activo
            FROM       TBL_ORGANO o
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            WHERE o.id_organo = @id
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<OrganoCore>(
            sql, new { id, tenantId });
    }

    public static async Task<int> OrganoInserta(
        DatosOrgano datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_ORGANO (nombre, descripcion, activo)
            VALUES (@Nombre, @Descripcion, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    public static async Task OrganoActualiza(
        DatosOrgano datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_ORGANO o
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            SET o.nombre      = @Nombre,
                o.descripcion = @Descripcion
            WHERE o.id_organo = @IdOrgano";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.Descripcion, datos.IdOrgano, tenantId
        });
    }

    public static async Task OrganoCambiaEstado(
        int id, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_ORGANO o
            INNER JOIN TBL_TENANT t ON t.id_tenant = @tenantId
            SET o.activo = @activo
            WHERE o.id_organo = @id";

        await conn.ExecuteAsync(sql, new { id, activo, tenantId });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIPO SOLICITUD
    // ═══════════════════════════════════════════════════════════════════════════

    public static async Task<IEnumerable<TipoSolicitudCore>> TipoSolicitudLista(
        int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT ts.id_tipo_solicitud AS IdTipoSolicitud,
                   ts.nombre            AS Nombre,
                   ts.descripcion       AS Descripcion,
                   ts.activo            AS Activo
            FROM       TBL_TIPO_SOLICITUD ts
            INNER JOIN TBL_TENANT         t ON t.id_tenant = @tenantId
            ORDER BY ts.nombre";

        return await conn.QueryAsync<TipoSolicitudCore>(sql, new { tenantId });
    }

    public static async Task<TipoSolicitudCore?> TipoSolicitudObtiene(
        int id, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT ts.id_tipo_solicitud AS IdTipoSolicitud,
                   ts.nombre            AS Nombre,
                   ts.descripcion       AS Descripcion,
                   ts.activo            AS Activo
            FROM       TBL_TIPO_SOLICITUD ts
            INNER JOIN TBL_TENANT         t ON t.id_tenant = @tenantId
            WHERE ts.id_tipo_solicitud = @id
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<TipoSolicitudCore>(
            sql, new { id, tenantId });
    }

    public static async Task<int> TipoSolicitudInserta(
        DatosTipoSolicitud datos, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_TIPO_SOLICITUD (nombre, descripcion, activo)
            VALUES (@Nombre, @Descripcion, @Activo);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos);
    }

    public static async Task TipoSolicitudActualiza(
        DatosTipoSolicitud datos, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_TIPO_SOLICITUD ts
            INNER JOIN TBL_TENANT     t ON t.id_tenant = @tenantId
            SET ts.nombre      = @Nombre,
                ts.descripcion = @Descripcion
            WHERE ts.id_tipo_solicitud = @IdTipoSolicitud";

        await conn.ExecuteAsync(sql, new
        {
            datos.Nombre, datos.Descripcion, datos.IdTipoSolicitud, tenantId
        });
    }

    public static async Task TipoSolicitudCambiaEstado(
        int id, bool activo, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_TIPO_SOLICITUD ts
            INNER JOIN TBL_TENANT     t ON t.id_tenant = @tenantId
            SET ts.activo = @activo
            WHERE ts.id_tipo_solicitud = @id";

        await conn.ExecuteAsync(sql, new { id, activo, tenantId });
    }
}
