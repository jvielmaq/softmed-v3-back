namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Payload de entrada para cambiar el estado de un examen.</summary>
public sealed class DatosCambiarEstado
{
    public int     ExamenId       { get; set; }
    public int     NuevoEstadoId  { get; set; }
    public string? Observacion    { get; set; }
}
