namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>DTO de entrada para sede.</summary>
public sealed class DatosSede
{
    public int?   IdSede      { get; set; }
    public string Nombre      { get; set; } = string.Empty;
    public string? Direccion  { get; set; }
    public string? Telefono   { get; set; }
    public int    IdInstitucion { get; set; }
    public bool   Activo      { get; set; } = true;
}
