namespace Softmed.V3.Softmed.Business.Modelo;

public sealed class DatosCambioMasivo
{
    public int[]   ExamenIds     { get; set; } = [];
    public int     NuevoEstadoId { get; set; }
    public string? Observacion   { get; set; }
}
