namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>DTO de entrada para pabellón.</summary>
public sealed class DatosPabellon
{
    public int?   IdPabellon { get; set; }
    public string Nombre     { get; set; } = string.Empty;
    public int    IdSede     { get; set; }
    public bool   Activo     { get; set; } = true;
}
