namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Proyección de salida para especialidad.</summary>
public sealed class EspecialidadCore
{
    public int    IdEspecialidad { get; set; }
    public string Nombre         { get; set; } = string.Empty;
    public bool   Activo         { get; set; }
}
