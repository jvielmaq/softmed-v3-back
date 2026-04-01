namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>DTO de entrada para órgano.</summary>
public sealed class DatosOrgano
{
    public int?   IdOrgano    { get; set; }
    public string Nombre      { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool   Activo      { get; set; } = true;
}
