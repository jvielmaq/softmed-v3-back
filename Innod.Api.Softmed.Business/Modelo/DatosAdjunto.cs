namespace Softmed.V3.Softmed.Business.Modelo;

public sealed class DatosAdjunto
{
    public int     ExamenId       { get; set; }
    public string  NombreArchivo  { get; set; } = string.Empty;
    public string  Url            { get; set; } = string.Empty;
    public string? TipoMime      { get; set; }
    public long    TamanoBytes    { get; set; }
}

public sealed class AdjuntoExamen
{
    public int      IdAdjunto      { get; set; }
    public int      IdExamen       { get; set; }
    public string   NombreArchivo  { get; set; } = string.Empty;
    public string   Url            { get; set; } = string.Empty;
    public string?  TipoMime      { get; set; }
    public long     TamanoBytes    { get; set; }
    public bool     Activo         { get; set; }
    public DateTime FechaCreacion  { get; set; }
}
