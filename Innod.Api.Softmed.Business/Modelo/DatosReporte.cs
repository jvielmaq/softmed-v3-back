namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Filtros para obtener datos del dashboard de reportes.</summary>
public sealed class FiltrosReporte
{
    public DateTime? FechaDesde    { get; set; }
    public DateTime? FechaHasta    { get; set; }
    public int?      InstitucionId { get; set; }
    public int?      SedeId        { get; set; }
}

/// <summary>KPIs del dashboard.</summary>
public sealed class DashboardKpi
{
    public int TotalExamenes      { get; set; }
    public int Pendientes         { get; set; }
    public int EnProceso          { get; set; }
    public int Firmados           { get; set; }
    public int Entregados         { get; set; }
    public int Rechazados         { get; set; }
    public int CreadosHoy         { get; set; }
    public int FirmadosHoy        { get; set; }
}

/// <summary>Fila de reporte de exámenes para exportar.</summary>
public sealed class ReporteExamenFila
{
    public int      IdExamen       { get; set; }
    public string   Barcode        { get; set; } = string.Empty;
    public string   Paciente       { get; set; } = string.Empty;
    public string   PacienteRut    { get; set; } = string.Empty;
    public string   Estado         { get; set; } = string.Empty;
    public string   Institucion    { get; set; } = string.Empty;
    public string   TipoSolicitud  { get; set; } = string.Empty;
    public DateTime FechaCreacion  { get; set; }
    public DateTime FechaMuestra   { get; set; }
    public DateTime? FechaFirma    { get; set; }
    public DateTime? FechaEntrega  { get; set; }
}
