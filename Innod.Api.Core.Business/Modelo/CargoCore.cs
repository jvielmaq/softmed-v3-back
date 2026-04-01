namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Proyección de salida para cargo.</summary>
public sealed class CargoCore
{
    public int    IdCargo { get; set; }
    public string Nombre  { get; set; } = string.Empty;
    public bool   Activo  { get; set; }
}
