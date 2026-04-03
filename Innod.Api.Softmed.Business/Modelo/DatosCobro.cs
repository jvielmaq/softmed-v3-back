namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload para registrar un cobro (Cemesi).</summary>
public sealed class DatosRegistrarCobro
{
    public int      ExamenId     { get; set; }
    public string   FormaPago    { get; set; } = string.Empty;
    public string   CodigoCobro  { get; set; } = string.Empty;
    public decimal  Valor        { get; set; }
    public string?  Observacion  { get; set; }
}

/// <summary>Cobro asociado a un examen.</summary>
public sealed class CobroExamen
{
    public int      IdCobro      { get; set; }
    public int      IdExamen     { get; set; }
    public string   FormaPago    { get; set; } = string.Empty;
    public string   CodigoCobro  { get; set; } = string.Empty;
    public decimal  Valor        { get; set; }
    public string?  Observacion  { get; set; }
    public DateTime FechaCreacion { get; set; }
}
