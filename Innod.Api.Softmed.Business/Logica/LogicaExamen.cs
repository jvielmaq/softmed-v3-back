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

    // Estado inicial al crear un examen (id=1 = "PENDIENTE", convención del sistema)
    private const int EstadoInicial = 1;

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
            datos
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

        return examen;
    }

    // ─── Crear ───────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosCrearExamen datos)
    {
        ValidadorExamen.Validar(datos);

        // Barcode único: "SM" + tenantId + timestamp milisegundos
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
                conn);

            await RepoExamen.InsertarExamenExtendido(
                idExamen,
                datos.Observaciones,
                datos.DiagnosticoPresuntivo,
                datos.MedicoSolicitante,
                datos.DatosAdicionales,
                conn);

            await RepoExamen.InsertarLog(
                idExamen,
                estadoAnterior: null,
                estadoNuevo:    EstadoInicial,
                _idUsuario, _ip,
                observacion: "Creación de examen",
                conn);

            await tx.CommitAsync();

            return new { idExamen, barcode, mensaje = "Examen creado correctamente." };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ─── CambiarEstado ───────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(DatosCambiarEstado datos)
    {
        if (datos.ExamenId <= 0)
            throw new ArgumentException("ExamenId es requerido.");
        if (datos.NuevoEstadoId <= 0)
            throw new ArgumentException("NuevoEstadoId es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var estadoActual = await RepoExamen.ObtenerEstadoActual(
            datos.ExamenId, _tenantId, conn);

        if (estadoActual is null)
            throw new KeyNotFoundException(
                $"Examen {datos.ExamenId} no encontrado en este tenant.");

        // Validar que la transición no retrocede
        if (datos.NuevoEstadoId <= estadoActual.Value)
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de {estadoActual} a {datos.NuevoEstadoId}: " +
                "no se permiten transiciones hacia atrás o al mismo estado.");

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await RepoExamen.ActualizarEstado(
                datos.ExamenId, datos.NuevoEstadoId, _tenantId, conn);

            await RepoExamen.InsertarLog(
                datos.ExamenId,
                estadoActual.Value,
                datos.NuevoEstadoId,
                _idUsuario, _ip,
                datos.Observacion,
                conn);

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

    public async Task<object> ObtenerEstados()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var estados = await RepoExamen.ObtenerEstados(conn);
        return estados;
    }
}
