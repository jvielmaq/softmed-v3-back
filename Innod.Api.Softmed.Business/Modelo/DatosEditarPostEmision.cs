namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload para edición post-emisión (examen ya firmado). Requiere motivo.</summary>
public sealed class DatosEditarPostEmision
{
    public int     ExamenId              { get; set; }
    public string  Motivo                { get; set; } = string.Empty;
    public string? Observaciones         { get; set; }
    public string? DiagnosticoPresuntivo { get; set; }
    public string? Macroscopia           { get; set; }
    public string? Microscopia           { get; set; }
    public string? Diagnostico           { get; set; }
    public string? Conclusion            { get; set; }
    public string? Histologia            { get; set; }
}
