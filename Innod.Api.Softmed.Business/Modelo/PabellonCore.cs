namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Proyección de salida para pabellón.</summary>
public sealed class PabellonCore
{
    public int    IdPabellon { get; set; }
    public string Nombre     { get; set; } = string.Empty;
    public int    IdSede     { get; set; }
    public string Sede       { get; set; } = string.Empty;
    public bool   Activo     { get; set; }
}
