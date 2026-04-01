namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>DTO de entrada para tipo de solicitud.</summary>
public sealed class DatosTipoSolicitud
{
    public int?   IdTipoSolicitud { get; set; }
    public string Nombre           { get; set; } = string.Empty;
    public string? Descripcion     { get; set; }
    public bool   Activo           { get; set; } = true;
}
