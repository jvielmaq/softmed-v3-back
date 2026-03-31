using Dapper;
using MySqlConnector;
using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Business.Repositorio;

public static class RepoExamen
{
    // ─── GRILLA ───────────────────────────────────────────────────────────────

    public static async Task<int> ContarGrilla(FiltrosGrilla f, int tenantId, MySqlConnection conn)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM       TBL_EXAMEN        e
            INNER JOIN TBL_INSTITUCION   i  ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT        t  ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId"
            + FiltrosDinamicos(f);

        return await conn.ExecuteScalarAsync<int>(sql, Parametros(f, tenantId));
    }

    public static async Task<IEnumerable<ExamenGrilla>> ObtenerGrilla(
        FiltrosGrilla f, int tenantId, MySqlConnection conn)
    {
        var sql = @"
            SELECT
                e.id_examen       AS IdExamen,
                e.barcode         AS Barcode,
                CONCAT(p.nombres, ' ', p.apellidos) AS Paciente,
                es.nombre_estado  AS Estado,
                e.id_estado       AS IdEstado,
                i.nombre          AS Institucion,
                e.fecha_creacion  AS FechaCreacion,
                e.fecha_muestra   AS FechaMuestra
            FROM       TBL_EXAMEN        e
            INNER JOIN TBL_PERSONA       p   ON p.id_persona     = e.id_paciente
            INNER JOIN TBL_ESTADO        es  ON es.id_estado     = e.id_estado
            INNER JOIN TBL_INSTITUCION   i   ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT        t   ON t.id_institucion = i.id_institucion
            WHERE t.id_tenant = @tenantId"
            + FiltrosDinamicos(f)
            + @"
            ORDER BY e.fecha_creacion DESC
            LIMIT @limit OFFSET @offset";

        var p = Parametros(f, tenantId);
        p.Add("limit",  f.RegistrosPorPagina);
        p.Add("offset", f.Offset);

        return await conn.QueryAsync<ExamenGrilla>(sql, p);
    }

    // ─── DETALLE ──────────────────────────────────────────────────────────────

    public static async Task<ExamenDetalle?> ObtenerPorId(
        int idExamen, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT
                e.id_examen               AS IdExamen,
                e.barcode                 AS Barcode,
                e.id_estado               AS IdEstado,
                es.nombre_estado          AS Estado,
                e.id_institucion          AS IdInstitucion,
                i.nombre                  AS Institucion,
                e.id_paciente             AS IdPaciente,
                CONCAT(p.nombres, ' ', p.apellidos) AS Paciente,
                e.tipo_solicitud_id       AS TipoSolicitudId,
                ts.nombre                 AS TipoSolicitud,
                e.fecha_creacion          AS FechaCreacion,
                e.fecha_muestra           AS FechaMuestra,
                ex.observaciones          AS Observaciones,
                ex.diagnostico_presuntivo AS DiagnosticoPresuntivo,
                ex.medico_solicitante     AS MedicoSolicitante,
                ex.datos_adicionales      AS DatosAdicionales
            FROM       TBL_EXAMEN           e
            INNER JOIN TBL_PERSONA          p   ON p.id_persona        = e.id_paciente
            INNER JOIN TBL_ESTADO           es  ON es.id_estado         = e.id_estado
            INNER JOIN TBL_INSTITUCION      i   ON i.id_institucion     = e.id_institucion
            INNER JOIN TBL_TENANT           t   ON t.id_institucion     = i.id_institucion
            INNER JOIN TBL_TIPO_SOLICITUD   ts  ON ts.id_tipo_solicitud = e.tipo_solicitud_id
            LEFT  JOIN TBL_EXAMEN_EXTENDIDO ex  ON ex.id_examen         = e.id_examen
            WHERE e.id_examen  = @idExamen
              AND t.id_tenant  = @tenantId
            LIMIT 1";

        return await conn.QueryFirstOrDefaultAsync<ExamenDetalle>(
            sql, new { idExamen, tenantId });
    }

    public static async Task<int?> ObtenerEstadoActual(
        int idExamen, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT e.id_estado
            FROM       TBL_EXAMEN      e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            WHERE e.id_examen = @idExamen
              AND t.id_tenant = @tenantId
            LIMIT 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { idExamen, tenantId });
    }

    // ─── ESTADOS ─────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> ObtenerEstados(MySqlConnection conn)
    {
        const string sql = @"
            SELECT id_estado AS IdEstado, nombre_estado AS NombreEstado, orden AS Orden
            FROM   TBL_ESTADO
            WHERE  activo = 1
            ORDER  BY orden";

        return await conn.QueryAsync(sql);
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> InsertarExamen(
        string barcode, int pacienteId, int institucionId,
        int tipoSolicitudId, DateTime fechaMuestra, int idEstadoInicial,
        MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_EXAMEN
                (barcode, id_paciente, id_institucion, tipo_solicitud_id,
                 fecha_muestra, id_estado, fecha_creacion)
            VALUES
                (@barcode, @pacienteId, @institucionId, @tipoSolicitudId,
                 @fechaMuestra, @idEstadoInicial, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(
            sql, new { barcode, pacienteId, institucionId,
                       tipoSolicitudId, fechaMuestra, idEstadoInicial });
    }

    public static async Task InsertarExamenExtendido(
        int idExamen, string? observaciones, string? diagnosticoPresuntivo,
        string? medicoSolicitante, string? datosAdicionales, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_EXAMEN_EXTENDIDO
                (id_examen, observaciones, diagnostico_presuntivo,
                 medico_solicitante, datos_adicionales)
            VALUES
                (@idExamen, @observaciones, @diagnosticoPresuntivo,
                 @medicoSolicitante, @datosAdicionales)";

        await conn.ExecuteAsync(sql, new
        {
            idExamen, observaciones, diagnosticoPresuntivo,
            medicoSolicitante, datosAdicionales
        });
    }

    public static async Task InsertarLog(
        int idExamen, int? estadoAnterior, int estadoNuevo,
        int idUsuario, string ip, string? observacion, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_LOG_EXAMEN
                (id_examen, estado_anterior, estado_nuevo,
                 id_usuario, ip, observacion, fecha)
            VALUES
                (@idExamen, @estadoAnterior, @estadoNuevo,
                 @idUsuario, @ip, @observacion, UTC_TIMESTAMP())";

        await conn.ExecuteAsync(sql, new
        {
            idExamen, estadoAnterior, estadoNuevo,
            idUsuario, ip, observacion
        });
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task ActualizarEstado(
        int idExamen, int nuevoEstadoId, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            UPDATE TBL_EXAMEN e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET e.id_estado = @nuevoEstadoId
            WHERE e.id_examen  = @idExamen
              AND t.id_tenant  = @tenantId";

        await conn.ExecuteAsync(sql, new { idExamen, nuevoEstadoId, tenantId });
    }

    // ─── PRIVADOS ─────────────────────────────────────────────────────────────

    private static string FiltrosDinamicos(FiltrosGrilla f)
    {
        var sb = new System.Text.StringBuilder();
        if (f.FechaDesde    is not null) sb.Append(" AND e.fecha_creacion >= @fechaDesde");
        if (f.FechaHasta    is not null) sb.Append(" AND e.fecha_creacion <= @fechaHasta");
        if (f.EstadoId      is not null) sb.Append(" AND e.id_estado = @estadoId");
        if (f.InstitucionId is not null) sb.Append(" AND e.id_institucion = @institucionId");
        return sb.ToString();
    }

    private static DynamicParameters Parametros(FiltrosGrilla f, int tenantId)
    {
        var p = new DynamicParameters();
        p.Add("tenantId", tenantId);
        if (f.FechaDesde    is not null) p.Add("fechaDesde",    f.FechaDesde);
        if (f.FechaHasta    is not null) p.Add("fechaHasta",    f.FechaHasta);
        if (f.EstadoId      is not null) p.Add("estadoId",      f.EstadoId);
        if (f.InstitucionId is not null) p.Add("institucionId", f.InstitucionId);
        return p;
    }
}
