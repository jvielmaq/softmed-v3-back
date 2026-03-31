namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload de entrada para crear un nuevo examen.</summary>
public sealed class DatosCrearExamen
{
    public int       PacienteId            { get; set; }
    public int       InstitucionId         { get; set; }
    public int       TipoSolicitudId       { get; set; }
    public DateTime  FechaMuestra          { get; set; }
    public string?   Observaciones         { get; set; }
    public string?   DiagnosticoPresuntivo { get; set; }
    public string?   MedicoSolicitante     { get; set; }
    public string?   DatosAdicionales      { get; set; }
}
