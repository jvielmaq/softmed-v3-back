using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Modelo;
using Softmed.V3.Softmed.Business.Repositorio;
using Softmed.V3.Softmed.Business.Validadores;

namespace Softmed.V3.Softmed.Business.Logica;

public sealed class LogicaExamen
{
    private readonly Solicitud _solicitud;
    private readonly int       _tenantId;
    private readonly int       _idUsuario;
    private readonly string    _ip;

    private const int EstadoInicial     = 1;  // CREADO
    private const int EstadoFirmado     = 17; // FIRMADO
    private const int EstadoEntregado   = 18; // ENTREGADO
    private const int EstadoRechazado   = 19; // RECHAZADO
    private const int EstadoAnulado     = 20; // ANULADO
    private const int EstadoReFirmado   = 21; // RE_FIRMADO
    private const int EstadoReEntregado = 22; // RE_ENTREGADO

    public LogicaExamen(Solicitud solicitud)
    {
        _solicitud = solicitud;
        _tenantId  = solicitud.TenantId;
        _idUsuario = solicitud.UserData?.IdUsuario ?? 0;
        _ip        = solicitud.Ip;
    }

    // ─── ObtenerGrilla ───────────────────────────────────────────────────────

    public async Task<object> ObtenerGrilla(FiltrosGrilla filtros)
    {
        if (filtros.RegistrosPorPagina <= 0) filtros.RegistrosPorPagina = 50;
        if (filtros.Pagina <= 0)             filtros.Pagina = 1;

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var total = await RepoExamen.ContarGrilla(filtros, _tenantId, conn);
        var datos = await RepoExamen.ObtenerGrilla(filtros, _tenantId, conn);

        return new
        {
            total,
            pagina = filtros.Pagina,
            data = datos
        };
    }

    // ─── ObtenerPorId ────────────────────────────────────────────────────────

    public async Task<object> ObtenerPorId(int idExamen)
    {
        if (idExamen <= 0)
            throw new ArgumentException("IdExamen debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var examen = await RepoExamen.ObtenerPorId(idExamen, _tenantId, conn);
        if (examen is null)
            throw new KeyNotFoundException(
                $"Examen {idExamen} no encontrado en este tenant.");

        var historial = await RepoExamen.ObtenerHistorial(idExamen, _tenantId, conn);

        return new { examen, historial };
    }

    // ─── Crear ───────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosCrearExamen datos)
    {
        ValidadorExamen.Validar(datos);

        var barcode = $"SM{_tenantId}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await using var tx   = await conn.BeginTransactionAsync();

        try
        {
            var idExamen = await RepoExamen.InsertarExamen(
                barcode,
                datos.PacienteId,
                datos.InstitucionId,
                datos.TipoSolicitudId,
                datos.FechaMuestra,
                EstadoInicial,
                conn, tx);

            await RepoExamen.InsertarExamenExtendido(
                idExamen,
                datos.Observaciones,
                datos.DiagnosticoPresuntivo,
                datos.MedicoSolicitante,
                datos.DatosAdicionales,
                conn, tx);

            await RepoExamen.InsertarLog(
                idExamen,
                estadoAnterior: null,
                estadoNuevo:    EstadoInicial,
                _idUsuario, _ip,
                observacion: "Creación de examen",
                conn, tx);

            await tx.CommitAsync();

            return new { idExamen, barcode, mensaje = "Examen creado correctamente." };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ─── Editar ──────────────────────────────────────────────────────────────

    public async Task<object> Editar(DatosEditarExamen datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(datos.ExamenId, _tenantId, conn);
        if (estadoActual is null)
            throw new KeyNotFoundException($"Examen {datos.ExamenId} no encontrado.");

        if (estadoActual >= EstadoFirmado)
            throw new InvalidOperationException(
                "No se puede editar un examen que ya fue firmado. Use edición post-emisión.");

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoExamen.ActualizarExamen(
                datos.ExamenId, datos.TipoSolicitudId, datos.FechaMuestra,
                _tenantId, conn, tx);

            await RepoExamen.ActualizarExamenExtendido(
                datos.ExamenId,
                datos.Observaciones, datos.DiagnosticoPresuntivo,
                datos.MedicoSolicitante, datos.DatosAdicionales,
                datos.Macroscopia, datos.Microscopia, datos.Diagnostico,
                datos.Conclusion, datos.Histologia,
                conn, tx);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var examen = await RepoExamen.ObtenerPorId(datos.ExamenId, _tenantId, conn);
        return new { examen, mensaje = "Examen actualizado correctamente." };
    }

    // ─── Firmar ──────────────────────────────────────────────────────────────

    public async Task<object> Firmar(DatosFirmar datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");

        var roles = _solicitud.UserData?.Roles ?? [];

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(datos.ExamenId, _tenantId, conn);
        if (estadoActual is null)
            throw new KeyNotFoundException($"Examen {datos.ExamenId} no encontrado.");

        // Determinar si es firma normal (→17) o re-firma (→21)
        var estadoDestino = estadoActual.Value == EstadoEntregado
            ? EstadoReFirmado
            : EstadoFirmado;

        // Validar transición contra TBL_ETAPA_ESTADO
        var transicionValida = await RepoExamen.ValidarTransicion(
            estadoActual.Value, estadoDestino, roles, conn);

        if (!transicionValida)
            throw new InvalidOperationException(
                $"No se puede firmar desde el estado actual ({estadoActual}). " +
                $"Roles: [{string.Join(", ", roles)}].");

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoExamen.FirmarExamen(
                datos.ExamenId, _idUsuario, _tenantId, conn, tx);

            // Actualizar al estado destino correcto (puede ser RE_FIRMADO)
            if (estadoDestino == EstadoReFirmado)
                await RepoExamen.ActualizarEstado(
                    datos.ExamenId, EstadoReFirmado, _tenantId, conn, tx);

            await RepoExamen.InsertarLog(
                datos.ExamenId,
                estadoActual.Value,
                estadoDestino,
                _idUsuario, _ip,
                datos.Observacion ?? "Examen firmado",
                conn, tx);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var examen = await RepoExamen.ObtenerPorId(datos.ExamenId, _tenantId, conn);
        return new { examen, mensaje = "Examen firmado correctamente." };
    }

    // ─── EditarPostEmision ───────────────────────────────────────────────────

    public async Task<object> EditarPostEmision(DatosEditarPostEmision datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Motivo) || datos.Motivo.Length < 10)
            throw new ArgumentException("Motivo es requerido (mínimo 10 caracteres).");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(datos.ExamenId, _tenantId, conn);
        if (estadoActual is null)
            throw new KeyNotFoundException($"Examen {datos.ExamenId} no encontrado.");

        if (estadoActual < EstadoFirmado)
            throw new InvalidOperationException(
                "La edición post-emisión solo aplica a exámenes firmados o entregados.");

        // Verificar feature flag edicion_examen
        if (!FeatureFlag.IsModuleActive("edicion_examen", _tenantId, conn))
            throw new FeatureFlagException("edicion_examen");

        // Obtener datos actuales para no perder campos no enviados
        var examenActual = await RepoExamen.ObtenerPorId(datos.ExamenId, _tenantId, conn)
            as ExamenDetalle;

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoExamen.ActualizarExamenExtendido(
                datos.ExamenId,
                datos.Observaciones ?? examenActual?.Observaciones,
                datos.DiagnosticoPresuntivo ?? examenActual?.DiagnosticoPresuntivo,
                examenActual?.MedicoSolicitante,
                examenActual?.DatosAdicionales,
                datos.Macroscopia ?? examenActual?.Macroscopia,
                datos.Microscopia ?? examenActual?.Microscopia,
                datos.Diagnostico ?? examenActual?.Diagnostico,
                datos.Conclusion ?? examenActual?.Conclusion,
                datos.Histologia ?? examenActual?.Histologia,
                conn, tx);

            var cambios = JsonSerializer.Serialize(datos);
            await RepoExamen.InsertarLogEdicion(
                datos.ExamenId, _idUsuario, datos.Motivo,
                cambios, _ip, pdfRegenerado: false,
                conn, tx);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var examen = await RepoExamen.ObtenerPorId(datos.ExamenId, _tenantId, conn);
        return new { examen, mensaje = "Examen editado post-emisión correctamente." };
    }

    // ─── CambiarEstado ───────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(DatosCambiarEstado datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");
        if (datos.NuevoEstadoId <= 0)
            throw new ArgumentException("NuevoEstadoId es requerido.");

        var roles = _solicitud.UserData?.Roles ?? [];

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(
            datos.ExamenId, _tenantId, conn);

        if (estadoActual is null)
            throw new KeyNotFoundException(
                $"Examen {datos.ExamenId} no encontrado en este tenant.");

        // Validar transición contra TBL_ETAPA_ESTADO + permisos del rol
        var transicionValida = await RepoExamen.ValidarTransicion(
            estadoActual.Value, datos.NuevoEstadoId, roles, conn);

        if (!transicionValida)
            throw new InvalidOperationException(
                $"Transición no permitida: estado {estadoActual} → {datos.NuevoEstadoId} " +
                $"para los roles [{string.Join(", ", roles)}].");

        // FIRMADO y RE_FIRMADO requieren endpoint FIRMAR
        if (datos.NuevoEstadoId == EstadoFirmado || datos.NuevoEstadoId == EstadoReFirmado)
            throw new InvalidOperationException(
                "Para firmar un examen utilice el endpoint FIRMAR.");

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoExamen.ActualizarEstado(
                datos.ExamenId, datos.NuevoEstadoId, _tenantId, conn, tx);

            if (datos.NuevoEstadoId == EstadoEntregado || datos.NuevoEstadoId == EstadoReEntregado)
                await RepoExamen.SetFechaEntrega(datos.ExamenId, _tenantId, conn, tx);

            await RepoExamen.InsertarLog(
                datos.ExamenId,
                estadoActual.Value,
                datos.NuevoEstadoId,
                _idUsuario, _ip,
                datos.Observacion,
                conn, tx);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var examen = await RepoExamen.ObtenerPorId(datos.ExamenId, _tenantId, conn);
        return new { examen, mensaje = "Estado actualizado correctamente." };
    }

    // ─── ObtenerEstados ──────────────────────────────────────────────────────

    public async Task<object> ObtenerEstados(int? examenId = null)
    {
        var roles = _solicitud.UserData?.Roles ?? [];

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        // Si se pasa un examenId, devolver solo los estados siguientes válidos
        if (examenId.HasValue && examenId.Value > 0)
        {
            var estadoActual = await RepoExamen.ObtenerEstadoActual(
                examenId.Value, _tenantId, conn);

            if (estadoActual is null)
                throw new KeyNotFoundException(
                    $"Examen {examenId.Value} no encontrado en este tenant.");

            var siguientes = await RepoExamen.ObtenerEstadosSiguientes(
                estadoActual.Value, roles, conn);

            return new
            {
                estadoActualId = estadoActual.Value,
                estadosSiguientes = siguientes
            };
        }

        // Sin examenId: devolver todos los estados (catálogo)
        var estados = await RepoExamen.ObtenerEstados(conn);
        return estados;
    }

    // ─── Reportes ────────────────────────────────────────────────────────────

    public async Task<object> ObtenerDashboard(FiltrosReporte filtros)
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var kpi = await RepoExamen.ObtenerDashboardKpi(filtros, _tenantId, conn);
        return kpi;
    }

    public async Task<object> ObtenerReporteExamenes(FiltrosReporte filtros)
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var datos = await RepoExamen.ObtenerReporteExamenes(filtros, _tenantId, conn);
        return new { total = datos.Count(), data = datos };
    }

    // ─── Cobros ──────────────────────────────────────────────────────────────

    public async Task<object> RegistrarCobro(DatosRegistrarCobro datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");
        if (string.IsNullOrWhiteSpace(datos.FormaPago))
            throw new ArgumentException("FormaPago es requerido.");
        if (string.IsNullOrWhiteSpace(datos.CodigoCobro))
            throw new ArgumentException("CodigoCobro es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(datos.ExamenId, _tenantId, conn);
        if (estadoActual is null)
            throw new KeyNotFoundException($"Examen {datos.ExamenId} no encontrado.");

        var idCobro = await RepoExamen.InsertarCobro(datos, conn);
        return new { idCobro, mensaje = "Cobro registrado correctamente." };
    }

    public async Task<object> ObtenerCobros(int idExamen)
    {
        if (idExamen <= 0)
            throw new ArgumentException("ExamenId debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var cobros = await RepoExamen.ObtenerCobrosPorExamen(idExamen, _tenantId, conn);
        return cobros;
    }

    // ─── Muestras ────────────────────────────────────────────────────────────

    public async Task<object> ObtenerMuestras(int idExamen)
    {
        if (idExamen <= 0) throw new ArgumentException("ExamenId requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoExamen.ObtenerMuestras(idExamen, conn);
    }

    public async Task<object> AgregarMuestra(DatosCrearMuestra datos)
    {
        if (datos.ExamenId <= 0) throw new ArgumentException("ExamenId requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await RepoExamen.InsertarMuestra(datos, conn);
        return new { idMuestra = id, mensaje = "Muestra agregada." };
    }

    public async Task<object> EliminarMuestra(int idMuestra)
    {
        if (idMuestra <= 0) throw new ArgumentException("IdMuestra requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoExamen.EliminarMuestra(idMuestra, conn);
        return new { mensaje = "Muestra eliminada." };
    }

    // ─── Busqueda Medicos ────────────────────────────────────────────────────

    public async Task<object> BuscarMedicos(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Length < 2)
            throw new ArgumentException("Texto de busqueda debe tener al menos 2 caracteres.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoExamen.BuscarMedicos(texto, _tenantId, conn);
    }

    // ─── Generar PDF (invoca Lambda V3 PDF) ─────────────────────────────────

    public async Task<object> GenerarPdf(int idExamen)
    {
        if (idExamen <= 0) throw new ArgumentException("ExamenId requerido.");

        using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(35) };
        var lambdaUrl = Environment.GetEnvironmentVariable("PDF_LAMBDA_URL")
            ?? "https://pdf-lambda-url-pending";

        var payload = System.Text.Json.JsonSerializer.Serialize(new { examId = idExamen, tenantId = _tenantId });
        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(lambdaUrl, content);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return System.Text.Json.JsonSerializer.Deserialize<object>(body) ?? (object)new { mensaje = "PDF generado." };
            return new { error = $"Lambda PDF: {response.StatusCode}", detalle = body };
        }
        catch (Exception ex)
        {
            return new { error = $"Error Lambda PDF: {ex.Message}" };
        }
    }

    // ─── Adjuntos ─────────────────────────────────────────────────────────────

    public async Task<object> ListarAdjuntos(int idExamen)
    {
        if (idExamen <= 0) throw new ArgumentException("ExamenId requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoExamen.ListarAdjuntos(idExamen, conn);
    }

    public async Task<object> AgregarAdjunto(DatosAdjunto datos)
    {
        if (datos.ExamenId <= 0) throw new ArgumentException("ExamenId requerido.");
        if (string.IsNullOrWhiteSpace(datos.NombreArchivo)) throw new ArgumentException("NombreArchivo requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await RepoExamen.InsertarAdjunto(datos, _idUsuario, conn);
        return new { idAdjunto = id, mensaje = "Adjunto agregado." };
    }

    public async Task<object> EliminarAdjunto(int idAdjunto)
    {
        if (idAdjunto <= 0) throw new ArgumentException("IdAdjunto requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoExamen.EliminarAdjunto(idAdjunto, conn);
        return new { mensaje = "Adjunto eliminado." };
    }

    // ─── Cambio Masivo ───────────────────────────────────────────────────────

    public async Task<object> CambiarEstadoMasivo(DatosCambioMasivo datos)
    {
        if (datos.ExamenIds.Length == 0)
            throw new ArgumentException("Se requiere al menos un ExamenId.");
        if (datos.NuevoEstadoId <= 0)
            throw new ArgumentException("NuevoEstadoId requerido.");

        if (datos.NuevoEstadoId == EstadoFirmado || datos.NuevoEstadoId == EstadoReFirmado)
            throw new InvalidOperationException(
                "Para firmar exámenes utilice el endpoint FIRMAR.");

        var roles = _solicitud.UserData?.Roles ?? [];

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var exitosos = 0;
        var errores = new List<string>();

        foreach (var idExamen in datos.ExamenIds)
        {
            try
            {
                var estadoActual = await RepoExamen.ObtenerEstadoActual(idExamen, _tenantId, conn);
                if (estadoActual is null) { errores.Add($"{idExamen}: no encontrado"); continue; }

                var valida = await RepoExamen.ValidarTransicion(
                    estadoActual.Value, datos.NuevoEstadoId, roles, conn);
                if (!valida)
                {
                    errores.Add($"{idExamen}: transición {estadoActual}→{datos.NuevoEstadoId} no permitida");
                    continue;
                }

                await RepoExamen.ActualizarEstado(idExamen, datos.NuevoEstadoId, _tenantId, conn);

                if (datos.NuevoEstadoId == EstadoEntregado || datos.NuevoEstadoId == EstadoReEntregado)
                    await RepoExamen.SetFechaEntrega(idExamen, _tenantId, conn);

                await RepoExamen.InsertarLog(idExamen, estadoActual.Value, datos.NuevoEstadoId,
                    _idUsuario, _ip, datos.Observacion ?? "Cambio masivo", conn);
                exitosos++;
            }
            catch (Exception ex)
            {
                errores.Add($"{idExamen}: {ex.Message}");
            }
        }

        return new { exitosos, errores = errores.Count, detalle = errores, mensaje = $"{exitosos} examenes actualizados." };
    }
}
