namespace Softmed.V3.Core.Business.Modelo;

/// <summary>DTO de entrada para operaciones de especialidad.</summary>
public sealed class DatosEspecialidad
{
    public int?   IdEspecialidad { get; set; }
    public string Nombre         { get; set; } = string.Empty;
    public bool   Activo         { get; set; } = true;
}
