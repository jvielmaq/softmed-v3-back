namespace Softmed.V3.Core.Business.Modelo;

/// <summary>DTO de entrada para operaciones de institución.</summary>
public sealed class DatosInstitucion
{
    public int?   IdInstitucion { get; set; }
    public string Nombre        { get; set; } = string.Empty;
    public string? Direccion    { get; set; }
    public string? Telefono     { get; set; }
    public string? Email        { get; set; }
    public bool   Activo        { get; set; } = true;
}
