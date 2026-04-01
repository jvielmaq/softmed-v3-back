namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Proyección de salida para órgano.</summary>
public sealed class OrganoCore
{
    public int    IdOrgano    { get; set; }
    public string Nombre      { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool   Activo      { get; set; }
}
