namespace Softmed.V3.Softmed.Business.Modelo;

public sealed class DatosCrearMuestra
{
    public int     ExamenId      { get; set; }
    public int?    TipoMuestraId { get; set; }
    public int?    OrganoId      { get; set; }
    public string? Descripcion   { get; set; }
    public int     Cantidad      { get; set; } = 1;
}

public sealed class MuestraExamen
{
    public int      IdMuestra     { get; set; }
    public int      IdExamen      { get; set; }
    public int?     IdTipoMuestra { get; set; }
    public string?  TipoMuestra   { get; set; }
    public int?     IdOrgano      { get; set; }
    public string?  Organo        { get; set; }
    public string?  Descripcion   { get; set; }
    public int      Cantidad      { get; set; }
    public bool     Activo        { get; set; }
}
