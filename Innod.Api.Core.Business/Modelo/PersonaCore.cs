namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Proyección de salida para persona.</summary>
public sealed class PersonaCore
{
    public int      IdPersona           { get; set; }
    public string   Nombres             { get; set; } = string.Empty;
    public string   Apellidos           { get; set; } = string.Empty;
    public string   Identificador       { get; set; } = string.Empty;
    public int      IdTipoIdentificador { get; set; }
    public string   TipoIdentificador   { get; set; } = string.Empty;
    public DateTime? FechaNacimiento    { get; set; }
    public int      IdGenero            { get; set; }
    public string   Genero              { get; set; } = string.Empty;
    public int      IdPrevision         { get; set; }
    public string   Prevision           { get; set; } = string.Empty;
    public string?  UrlImagen           { get; set; }
    public string?  UrlFirma            { get; set; }
    public bool     Activo              { get; set; }
}
