namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>DTO de entrada para tipo de muestra.</summary>
public sealed class DatosTipoMuestra
{
    public int?   IdTipoMuestra { get; set; }
    public string Nombre        { get; set; } = string.Empty;
    public string? Descripcion  { get; set; }
    public bool   Activo        { get; set; } = true;
}
