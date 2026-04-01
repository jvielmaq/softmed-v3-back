namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Proyección de salida para institución.</summary>
public sealed class InstitucionCore
{
    public int    IdInstitucion { get; set; }
    public string Nombre        { get; set; } = string.Empty;
    public string? Direccion    { get; set; }
    public string? Telefono     { get; set; }
    public string? Email        { get; set; }
    public bool   Activo        { get; set; }
}
