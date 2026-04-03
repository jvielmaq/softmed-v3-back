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
            INNER JOIN TBL_PERSONA       p  ON p.id_persona     = e.id_paciente
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
                es.nombre         AS Estado,
                e.id_estado       AS IdEstado,
                i.nombre          AS Institucion,
                ts.nombre         AS TipoSolicitud,
                e.fecha_creacion  AS FechaCreacion,
                e.fecha_muestra   AS FechaMuestra
            FROM       TBL_EXAMEN        e
            INNER JOIN TBL_PERSONA       p   ON p.id_persona        = e.id_paciente
            INNER JOIN TBL_ESTADO        es  ON es.id_estado         = e.id_estado
            INNER JOIN TBL_INSTITUCION   i   ON i.id_institucion     = e.id_institucion
            INNER JOIN TBL_TENANT        t   ON t.id_institucion     = i.id_institucion
            INNER JOIN TBL_TIPO_SOLICITUD ts ON ts.id_tipo_solicitud = e.id_tipo_solicitud
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
                es.nombre                 AS Estado,
                e.id_institucion          AS IdInstitucion,
                i.nombre                  AS Institucion,
                e.id_paciente             AS IdPaciente,
                CONCAT(p.nombres, ' ', p.apellidos) AS Paciente,
                p.identificador           AS PacienteRut,
                e.id_tipo_solicitud       AS TipoSolicitudId,
                ts.nombre                 AS TipoSolicitud,
                e.fecha_creacion          AS FechaCreacion,
                e.fecha_muestra           AS FechaMuestra,
                e.fecha_recepcion         AS FechaRecepcion,
                e.fecha_firma             AS FechaFirma,
                e.fecha_entrega           AS FechaEntrega,
                e.url_pdf                 AS UrlPdf,
                COALESCE(e.critico, 0)    AS Critico,
                COALESCE(e.urgente, 0)    AS Urgente,
                e.id_signatario           AS IdSignatario,
                CONCAT(ps.nombres, ' ', ps.apellidos) AS Signatario,
                ex.observaciones          AS Observaciones,
                ex.diagnostico_presuntivo AS DiagnosticoPresuntivo,
                ex.medico_solicitante     AS MedicoSolicitante,
                ex.datos_adicionales      AS DatosAdicionales,
                ex.macroscopia            AS Macroscopia,
                ex.microscopia            AS Microscopia,
                ex.diagnostico            AS Diagnostico,
                ex.conclusion             AS Conclusion,
                ex.histologia             AS Histologia
            FROM       TBL_EXAMEN           e
            INNER JOIN TBL_PERSONA          p   ON p.id_persona        = e.id_paciente
            INNER JOIN TBL_ESTADO           es  ON es.id_estado         = e.id_estado
            INNER JOIN TBL_INSTITUCION      i   ON i.id_institucion     = e.id_institucion
            INNER JOIN TBL_TENANT           t   ON t.id_institucion     = i.id_institucion
            INNER JOIN TBL_TIPO_SOLICITUD   ts  ON ts.id_tipo_solicitud = e.id_tipo_solicitud
            LEFT  JOIN TBL_EXAMEN_EXTENDIDO ex  ON ex.id_examen         = e.id_examen
            LEFT  JOIN TBL_USUARIO          us  ON us.id_usuario        = e.id_signatario
            LEFT  JOIN TBL_PERSONA          ps  ON ps.id_persona        = us.id_persona
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
            SELECT id_estado AS IdEstado, nombre AS NombreEstado, orden AS Orden
            FROM   TBL_ESTADO
            WHERE  activo = 1
            ORDER  BY orden";

        return await conn.QueryAsync(sql);
    }

    // ─── TRANSICIONES DE ESTADO ─────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> ObtenerEstadosSiguientes(
        int estadoActualId, string[] roles, MySqlConnection conn)
    {
        const string sql = @"
            SELECT DISTINCT
                ee.id_estado_destino AS IdEstado,
                es.nombre            AS NombreEstado,
                es.orden             AS Orden,
                ee.descripcion       AS Descripcion
            FROM   TBL_ETAPA_ESTADO ee
            INNER JOIN TBL_ESTADO   es ON es.id_estado = ee.id_estado_destino
            WHERE  ee.id_estado_origen = @estadoActualId
              AND  ee.rol IN @roles
              AND  ee.activo = 1
              AND  es.activo = 1
            ORDER BY es.orden";

        return await conn.QueryAsync(sql, new { estadoActualId, roles });
    }

    public static async Task<bool> ValidarTransicion(
        int estadoOrigenId, int estadoDestinoId, string[] roles, MySqlConnection conn)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM   TBL_ETAPA_ESTADO
            WHERE  id_estado_origen  = @estadoOrigenId
              AND  id_estado_destino = @estadoDestinoId
              AND  rol IN @roles
              AND  activo = 1";

        var count = await conn.ExecuteScalarAsync<int>(
            sql, new { estadoOrigenId, estadoDestinoId, roles });
        return count > 0;
    }

    // ─── INSERT ───────────────────────────────────────────────────────────────

    public static async Task<int> InsertarExamen(
        string barcode, int pacienteId, int institucionId,
        int tipoSolicitudId, DateTime fechaMuestra, int idEstadoInicial,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_EXAMEN
                (barcode, id_paciente, id_institucion, id_tipo_solicitud,
                 fecha_muestra, id_estado)
            VALUES
                (@barcode, @pacienteId, @institucionId, @tipoSolicitudId,
                 @fechaMuestra, @idEstadoInicial);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(
            sql, new { barcode, pacienteId, institucionId,
                       tipoSolicitudId, fechaMuestra, idEstadoInicial }, tx);
    }

    public static async Task InsertarExamenExtendido(
        int idExamen, string? observaciones, string? diagnosticoPresuntivo,
        string? medicoSolicitante, string? datosAdicionales,
        MySqlConnection conn, MySqlTransaction? tx = null)
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
        }, tx);
    }

    public static async Task InsertarLog(
        int idExamen, int? estadoAnterior, int estadoNuevo,
        int idUsuario, string ip, string? observacion,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_LOG_EXAMEN
                (id_examen, estado_anterior, estado_nuevo,
                 id_usuario, ip, observacion)
            VALUES
                (@idExamen, @estadoAnterior, @estadoNuevo,
                 @idUsuario, @ip, @observacion)";

        await conn.ExecuteAsync(sql, new
        {
            idExamen, estadoAnterior, estadoNuevo,
            idUsuario, ip, observacion
        }, tx);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public static async Task ActualizarEstado(
        int idExamen, int nuevoEstadoId, int tenantId,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            UPDATE TBL_EXAMEN e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET e.id_estado = @nuevoEstadoId
            WHERE e.id_examen  = @idExamen
              AND t.id_tenant  = @tenantId";

        await conn.ExecuteAsync(sql, new { idExamen, nuevoEstadoId, tenantId }, tx);
    }

    public static async Task ActualizarExamen(
        int idExamen, int? tipoSolicitudId, DateTime? fechaMuestra,
        int tenantId, MySqlConnection conn, MySqlTransaction? tx = null)
    {
        var sets = new List<string>();
        var p = new DynamicParameters();
        p.Add("idExamen", idExamen);
        p.Add("tenantId", tenantId);

        if (tipoSolicitudId.HasValue)
        {
            sets.Add("e.id_tipo_solicitud = @tipoSolicitudId");
            p.Add("tipoSolicitudId", tipoSolicitudId.Value);
        }
        if (fechaMuestra.HasValue)
        {
            sets.Add("e.fecha_muestra = @fechaMuestra");
            p.Add("fechaMuestra", fechaMuestra.Value);
        }

        if (sets.Count == 0) return;

        var sql = $@"
            UPDATE TBL_EXAMEN e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET {string.Join(", ", sets)}
            WHERE e.id_examen = @idExamen
              AND t.id_tenant = @tenantId";

        await conn.ExecuteAsync(sql, p, tx);
    }

    public static async Task ActualizarExamenExtendido(
        int idExamen, string? observaciones, string? diagnosticoPresuntivo,
        string? medicoSolicitante, string? datosAdicionales,
        string? macroscopia, string? microscopia, string? diagnostico,
        string? conclusion, string? histologia,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_EXAMEN_EXTENDIDO
                (id_examen, observaciones, diagnostico_presuntivo,
                 medico_solicitante, datos_adicionales,
                 macroscopia, microscopia, diagnostico, conclusion, histologia)
            VALUES
                (@idExamen, @observaciones, @diagnosticoPresuntivo,
                 @medicoSolicitante, @datosAdicionales,
                 @macroscopia, @microscopia, @diagnostico, @conclusion, @histologia)
            ON DUPLICATE KEY UPDATE
                observaciones          = @observaciones,
                diagnostico_presuntivo = @diagnosticoPresuntivo,
                medico_solicitante     = @medicoSolicitante,
                datos_adicionales      = @datosAdicionales,
                macroscopia            = @macroscopia,
                microscopia            = @microscopia,
                diagnostico            = @diagnostico,
                conclusion             = @conclusion,
                histologia             = @histologia";

        await conn.ExecuteAsync(sql, new
        {
            idExamen, observaciones, diagnosticoPresuntivo,
            medicoSolicitante, datosAdicionales,
            macroscopia, microscopia, diagnostico, conclusion, histologia
        }, tx);
    }

    public static async Task FirmarExamen(
        int idExamen, int idSignatario, int tenantId,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            UPDATE TBL_EXAMEN e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET e.id_estado      = @estadoFirmado,
                e.fecha_firma    = UTC_TIMESTAMP(),
                e.id_signatario  = @idSignatario
            WHERE e.id_examen = @idExamen
              AND t.id_tenant = @tenantId";

        await conn.ExecuteAsync(sql, new { idExamen, idSignatario, tenantId, estadoFirmado = 17 }, tx);
    }

    public static async Task SetFechaEntrega(
        int idExamen, int tenantId,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            UPDATE TBL_EXAMEN e
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion = i.id_institucion
            SET e.fecha_entrega = UTC_TIMESTAMP()
            WHERE e.id_examen = @idExamen
              AND t.id_tenant = @tenantId";

        await conn.ExecuteAsync(sql, new { idExamen, tenantId }, tx);
    }

    public static async Task InsertarLogEdicion(
        int idExamen, int idUsuario, string motivo, string cambiosJson,
        string ip, bool pdfRegenerado,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_LOG_EDICION_EXAMEN
                (id_examen, id_usuario, motivo, cambios, ip, pdf_regenerado)
            VALUES
                (@idExamen, @idUsuario, @motivo, @cambiosJson, @ip, @pdfRegenerado)";

        await conn.ExecuteAsync(sql, new
        {
            idExamen, idUsuario, motivo, cambiosJson, ip, pdfRegenerado
        }, tx);
    }

    // ─── REPORTES ─────────────────────────────────────────────────────────────

    public static async Task<DashboardKpi> ObtenerDashboardKpi(
        FiltrosReporte f, int tenantId, MySqlConnection conn)
    {
        var whereExtra = "";
        var p = new DynamicParameters();
        p.Add("tenantId", tenantId);

        if (f.FechaDesde.HasValue)    { whereExtra += " AND e.fecha_creacion >= @fechaDesde";    p.Add("fechaDesde", f.FechaDesde); }
        if (f.FechaHasta.HasValue)    { whereExtra += " AND e.fecha_creacion <= @fechaHasta";    p.Add("fechaHasta", f.FechaHasta); }
        if (f.InstitucionId.HasValue) { whereExtra += " AND e.id_institucion = @institucionId"; p.Add("institucionId", f.InstitucionId); }

        // Usar id_estado (1=PENDIENTE, 2=EN_PROCESO, 3=FIRMADO, 4=ENTREGADO, 5=RECHAZADO)
        var sql = $@"
            SELECT
                COUNT(*)                                                          AS TotalExamenes,
                SUM(CASE WHEN e.id_estado BETWEEN 1 AND 6 THEN 1 ELSE 0 END)      AS Pendientes,
                SUM(CASE WHEN e.id_estado BETWEEN 7 AND 15 THEN 1 ELSE 0 END)    AS EnProceso,
                SUM(CASE WHEN e.id_estado IN (16, 20) THEN 1 ELSE 0 END)          AS Firmados,
                SUM(CASE WHEN e.id_estado IN (17, 21) THEN 1 ELSE 0 END)          AS Entregados,
                SUM(CASE WHEN e.id_estado IN (18, 19) THEN 1 ELSE 0 END)          AS Rechazados,
                SUM(CASE WHEN DATE(e.fecha_creacion) = CURDATE() THEN 1 ELSE 0 END) AS CreadosHoy,
                SUM(CASE WHEN DATE(e.fecha_firma) = CURDATE() THEN 1 ELSE 0 END)    AS FirmadosHoy
            FROM       TBL_EXAMEN      e
            INNER JOIN TBL_INSTITUCION i  ON i.id_institucion  = e.id_institucion
            INNER JOIN TBL_TENANT      t  ON t.id_institucion  = i.id_institucion
            WHERE t.id_tenant = @tenantId {whereExtra}";

        return await conn.QueryFirstAsync<DashboardKpi>(sql, p);
    }

    public static async Task<IEnumerable<ReporteExamenFila>> ObtenerReporteExamenes(
        FiltrosReporte f, int tenantId, MySqlConnection conn)
    {
        var whereExtra = "";
        var p = new DynamicParameters();
        p.Add("tenantId", tenantId);

        if (f.FechaDesde.HasValue)    { whereExtra += " AND e.fecha_creacion >= @fechaDesde";    p.Add("fechaDesde", f.FechaDesde); }
        if (f.FechaHasta.HasValue)    { whereExtra += " AND e.fecha_creacion <= @fechaHasta";    p.Add("fechaHasta", f.FechaHasta); }
        if (f.InstitucionId.HasValue) { whereExtra += " AND e.id_institucion = @institucionId"; p.Add("institucionId", f.InstitucionId); }

        var sql = $@"
            SELECT
                e.id_examen       AS IdExamen,
                e.barcode         AS Barcode,
                CONCAT(pe.nombres, ' ', pe.apellidos) AS Paciente,
                pe.identificador  AS PacienteRut,
                es.nombre         AS Estado,
                i.nombre          AS Institucion,
                ts.nombre         AS TipoSolicitud,
                e.fecha_creacion  AS FechaCreacion,
                e.fecha_muestra   AS FechaMuestra,
                e.fecha_firma     AS FechaFirma,
                e.fecha_entrega   AS FechaEntrega
            FROM       TBL_EXAMEN         e
            INNER JOIN TBL_PERSONA        pe ON pe.id_persona       = e.id_paciente
            INNER JOIN TBL_ESTADO         es ON es.id_estado         = e.id_estado
            INNER JOIN TBL_INSTITUCION    i  ON i.id_institucion     = e.id_institucion
            INNER JOIN TBL_TENANT         t  ON t.id_institucion     = i.id_institucion
            INNER JOIN TBL_TIPO_SOLICITUD ts ON ts.id_tipo_solicitud = e.id_tipo_solicitud
            WHERE t.id_tenant = @tenantId {whereExtra}
            ORDER BY e.fecha_creacion DESC
            LIMIT 5000";

        return await conn.QueryAsync<ReporteExamenFila>(sql, p);
    }

    // ─── COBROS ───────────────────────────────────────────────────────────────

    public static async Task<int> InsertarCobro(
        DatosRegistrarCobro datos,
        MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_COBROS_EXAMEN
                (id_examen, forma_pago, codigo_cobro, valor, observacion)
            VALUES
                (@ExamenId, @FormaPago, @CodigoCobro, @Valor, @Observacion);
            SELECT LAST_INSERT_ID();";

        return await conn.ExecuteScalarAsync<int>(sql, datos, tx);
    }

    public static async Task<IEnumerable<CobroExamen>> ObtenerCobrosPorExamen(
        int idExamen, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT c.id_cobro AS IdCobro, c.id_examen AS IdExamen,
                   c.forma_pago AS FormaPago, c.codigo_cobro AS CodigoCobro,
                   c.valor AS Valor, c.observacion AS Observacion,
                   c.created_at AS FechaCreacion
            FROM   TBL_COBROS_EXAMEN  c
            INNER JOIN TBL_EXAMEN      e ON e.id_examen      = c.id_examen
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion  = e.id_institucion
            INNER JOIN TBL_TENANT      t ON t.id_institucion  = i.id_institucion
            WHERE  c.id_examen = @idExamen
              AND  t.id_tenant = @tenantId
            ORDER  BY c.created_at DESC";

        return await conn.QueryAsync<CobroExamen>(sql, new { idExamen, tenantId });
    }

    // ─── HISTORIAL LOG ────────────────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> ObtenerHistorial(
        int idExamen, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT l.id_log AS IdLog, l.estado_anterior AS EstadoAnterior,
                   l.estado_nuevo AS EstadoNuevo, l.observacion AS Observacion,
                   l.ip AS Ip, l.created_at AS FechaCreacion,
                   CONCAT(pe.nombres, ' ', pe.apellidos) AS Usuario
            FROM       TBL_LOG_EXAMEN l
            INNER JOIN TBL_EXAMEN     e  ON e.id_examen  = l.id_examen
            INNER JOIN TBL_INSTITUCION i ON i.id_institucion = e.id_institucion
            INNER JOIN TBL_TENANT     t  ON t.id_institucion = i.id_institucion
            INNER JOIN TBL_USUARIO    u  ON u.id_usuario = l.id_usuario
            INNER JOIN TBL_PERSONA    pe ON pe.id_persona = u.id_persona
            WHERE l.id_examen = @idExamen
              AND t.id_tenant = @tenantId
            ORDER BY l.created_at DESC";

        return await conn.QueryAsync(sql, new { idExamen, tenantId });
    }

    // ─── ADJUNTOS ──────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<AdjuntoExamen>> ListarAdjuntos(int idExamen, MySqlConnection conn)
    {
        const string sql = @"
            SELECT id_adjunto AS IdAdjunto, id_examen AS IdExamen,
                   nombre_archivo AS NombreArchivo, url AS Url,
                   tipo_mime AS TipoMime, tamano_bytes AS TamanoBytes,
                   activo AS Activo, created_at AS FechaCreacion
            FROM TBL_EXAMEN_ADJUNTO
            WHERE id_examen = @idExamen AND activo = 1
            ORDER BY created_at DESC";
        return await conn.QueryAsync<AdjuntoExamen>(sql, new { idExamen });
    }

    public static async Task<int> InsertarAdjunto(DatosAdjunto datos, int idUsuario, MySqlConnection conn)
    {
        const string sql = @"
            INSERT INTO TBL_EXAMEN_ADJUNTO (id_examen, nombre_archivo, url, tipo_mime, tamano_bytes, id_usuario_subio, activo)
            VALUES (@ExamenId, @NombreArchivo, @Url, @TipoMime, @TamanoBytes, @idUsuario, 1);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            datos.ExamenId, datos.NombreArchivo, datos.Url, datos.TipoMime, datos.TamanoBytes, idUsuario
        });
    }

    public static async Task EliminarAdjunto(int idAdjunto, MySqlConnection conn)
    {
        await conn.ExecuteAsync("UPDATE TBL_EXAMEN_ADJUNTO SET activo = 0 WHERE id_adjunto = @idAdjunto",
            new { idAdjunto });
    }

    // ─── MUESTRAS ──────────────────────────────────────────────────────────────

    public static async Task<IEnumerable<MuestraExamen>> ObtenerMuestras(
        int idExamen, MySqlConnection conn)
    {
        const string sql = @"
            SELECT m.id_muestra AS IdMuestra, m.id_examen AS IdExamen,
                   m.id_tipo_muestra AS IdTipoMuestra, tm.nombre AS TipoMuestra,
                   m.id_organo AS IdOrgano, o.nombre AS Organo,
                   m.descripcion AS Descripcion, m.cantidad AS Cantidad, m.activo AS Activo
            FROM       TBL_MUESTRA      m
            LEFT  JOIN TBL_TIPO_MUESTRA tm ON tm.id_tipo_muestra = m.id_tipo_muestra
            LEFT  JOIN TBL_ORGANO       o  ON o.id_organo        = m.id_organo
            WHERE m.id_examen = @idExamen AND m.activo = 1
            ORDER BY m.id_muestra";
        return await conn.QueryAsync<MuestraExamen>(sql, new { idExamen });
    }

    public static async Task<int> InsertarMuestra(
        DatosCrearMuestra datos, MySqlConnection conn, MySqlTransaction? tx = null)
    {
        const string sql = @"
            INSERT INTO TBL_MUESTRA (id_examen, id_tipo_muestra, id_organo, descripcion, cantidad, activo)
            VALUES (@ExamenId, @TipoMuestraId, @OrganoId, @Descripcion, @Cantidad, 1);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, datos, tx);
    }

    public static async Task EliminarMuestra(int idMuestra, MySqlConnection conn)
    {
        await conn.ExecuteAsync("UPDATE TBL_MUESTRA SET activo = 0 WHERE id_muestra = @idMuestra",
            new { idMuestra });
    }

    // ─── BUSQUEDA MEDICOS ─────────────────────────────────────────────────────

    public static async Task<IEnumerable<dynamic>> BuscarMedicos(
        string texto, int tenantId, MySqlConnection conn)
    {
        const string sql = @"
            SELECT DISTINCT
                p.id_persona AS IdPersona,
                p.nombres AS Nombres, p.apellidos AS Apellidos,
                p.identificador AS Identificador,
                c.nombre AS Cargo, es.nombre AS Especialidad
            FROM       TBL_EMPLEADO         e
            INNER JOIN TBL_PERSONA          p   ON p.id_persona   = e.id_persona
            LEFT  JOIN TBL_EMPLEADO_EMPLEOS ee  ON ee.id_empleado = e.id_empleado AND ee.activo = 1
            LEFT  JOIN TBL_CARGO            c   ON c.id_cargo     = ee.id_cargo
            LEFT  JOIN TBL_ESPECIALIDAD     es  ON es.id_especialidad = ee.id_especialidad
            WHERE e.activo = 1
              AND (p.nombres LIKE @texto OR p.apellidos LIKE @texto OR p.identificador LIKE @texto)
            ORDER BY p.apellidos, p.nombres
            LIMIT 10";
        return await conn.QueryAsync(sql, new { texto = $"%{texto}%", tenantId });
    }

    // ─── PRIVADOS ─────────────────────────────────────────────────────────────

    private static string FiltrosDinamicos(FiltrosGrilla f)
    {
        var sb = new System.Text.StringBuilder();
        if (f.FechaDesde    is not null) sb.Append(" AND e.fecha_creacion >= @fechaDesde");
        if (f.FechaHasta    is not null) sb.Append(" AND e.fecha_creacion <= @fechaHasta");
        if (f.EstadoId      is not null) sb.Append(" AND e.id_estado = @estadoId");
        if (f.InstitucionId is not null) sb.Append(" AND e.id_institucion = @institucionId");
        if (f.PacienteId    is not null) sb.Append(" AND e.id_paciente = @pacienteId");
        if (!string.IsNullOrWhiteSpace(f.Busqueda))
            sb.Append(" AND (e.barcode LIKE @busqueda OR p.nombres LIKE @busqueda OR p.apellidos LIKE @busqueda OR p.identificador LIKE @busqueda)");
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
        if (f.PacienteId    is not null) p.Add("pacienteId",    f.PacienteId);
        if (!string.IsNullOrWhiteSpace(f.Busqueda)) p.Add("busqueda", $"%{f.Busqueda}%");
        return p;
    }
}
