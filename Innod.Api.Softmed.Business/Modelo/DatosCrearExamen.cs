namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload de entrada para crear un nuevo examen (solicitud completa).</summary>
public sealed class DatosCrearExamen
{
    // ── Datos del paciente ────────────────────────────────────────────────────
    public int       PacienteId            { get; set; }

    // ── Datos de la solicitud ─────────────────────────────────────────────────
    public int       InstitucionId         { get; set; }
    public int?      SedeId                { get; set; }
    public int       TipoSolicitudId       { get; set; }
    public string?   MedicoSolicitante     { get; set; }
    public string?   DiagnosticoPresuntivo { get; set; }

    // ── Datos de la muestra ───────────────────────────────────────────────────
    public int?      OrganoId              { get; set; }
    public int?      TipoMuestraId         { get; set; }
    public DateTime  FechaMuestra          { get; set; }
    public DateTime? FechaRecepcion        { get; set; }

    // ── Extras ────────────────────────────────────────────────────────────────
    public bool      Critico               { get; set; }
    public string?   Observaciones         { get; set; }
    public string?   DatosAdicionales      { get; set; }
}
