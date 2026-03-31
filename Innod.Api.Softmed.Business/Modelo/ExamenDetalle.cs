namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Vista completa de un examen incluyendo datos de TBL_EXAMEN_EXTENDIDO.</summary>
public sealed class ExamenDetalle
{
    // ── TBL_EXAMEN ──────────────────────────────────────────────────────────
    public int      IdExamen          { get; set; }
    public string   Barcode           { get; set; } = string.Empty;
    public int      IdEstado          { get; set; }
    public string   Estado            { get; set; } = string.Empty;
    public int      IdInstitucion     { get; set; }
    public string   Institucion       { get; set; } = string.Empty;
    public int      IdPaciente        { get; set; }
    public string   Paciente          { get; set; } = string.Empty;
    public int      TipoSolicitudId   { get; set; }
    public string   TipoSolicitud     { get; set; } = string.Empty;
    public DateTime FechaCreacion     { get; set; }
    public DateTime FechaMuestra      { get; set; }

    // ── TBL_EXAMEN_EXTENDIDO ─────────────────────────────────────────────────
    public string?  Observaciones     { get; set; }
    public string?  DiagnosticoPresuntivo { get; set; }
    public string?  MedicoSolicitante { get; set; }
    public string?  DatosAdicionales  { get; set; }
}
