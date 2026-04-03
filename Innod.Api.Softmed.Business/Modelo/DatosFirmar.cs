namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload para firmar un examen.</summary>
public sealed class DatosFirmar
{
    public int     ExamenId    { get; set; }
    public string? Observacion { get; set; }
}
