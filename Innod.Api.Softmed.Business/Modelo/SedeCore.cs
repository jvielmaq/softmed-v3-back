namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Proyección de salida para sede.</summary>
public sealed class SedeCore
{
    public int    IdSede        { get; set; }
    public string Nombre        { get; set; } = string.Empty;
    public string? Direccion    { get; set; }
    public string? Telefono     { get; set; }
    public int    IdInstitucion { get; set; }
    public string Institucion   { get; set; } = string.Empty;
    public bool   Activo        { get; set; }
}
