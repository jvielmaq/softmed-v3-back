namespace Softmed.V3.Core.Business.Modelo;

/// <summary>DTO de entrada para operaciones de persona.</summary>
public sealed class DatosPersona
{
    public int?      IdPersona           { get; set; }
    public string    Nombres             { get; set; } = string.Empty;
    public string    Apellidos           { get; set; } = string.Empty;
    public string    Identificador       { get; set; } = string.Empty;
    public int       IdTipoIdentificador { get; set; }
    public DateTime? FechaNacimiento     { get; set; }
    public int       IdGenero            { get; set; }
    public int       IdPrevision         { get; set; }
    public string?   UrlImagen           { get; set; }
    public string?   UrlFirma            { get; set; }
    public bool      Activo              { get; set; } = true;
}
