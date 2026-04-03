namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Proyección liviana para la grilla de listado de exámenes.</summary>
public sealed class ExamenGrilla
{
    public int      IdExamen       { get; set; }
    public string   Barcode        { get; set; } = string.Empty;
    public string   Paciente       { get; set; } = string.Empty;
    public string   Estado         { get; set; } = string.Empty;
    public int      IdEstado       { get; set; }
    public string   Institucion    { get; set; } = string.Empty;
    public string   TipoSolicitud  { get; set; } = string.Empty;
    public DateTime FechaCreacion  { get; set; }
    public DateTime FechaMuestra   { get; set; }
}
