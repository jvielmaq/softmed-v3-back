namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload para editar un examen antes de firma.</summary>
public sealed class DatosEditarExamen
{
    public int       ExamenId              { get; set; }
    public int?      TipoSolicitudId       { get; set; }
    public DateTime? FechaMuestra          { get; set; }
    public string?   Observaciones         { get; set; }
    public string?   DiagnosticoPresuntivo { get; set; }
    public string?   MedicoSolicitante     { get; set; }
    public string?   DatosAdicionales      { get; set; }
    public string?   Macroscopia           { get; set; }
    public string?   Microscopia           { get; set; }
    public string?   Diagnostico           { get; set; }
    public string?   Conclusion            { get; set; }
    public string?   Histologia            { get; set; }
}
