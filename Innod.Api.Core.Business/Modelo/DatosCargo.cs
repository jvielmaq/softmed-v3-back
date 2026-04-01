namespace Softmed.V3.Core.Business.Modelo;

/// <summary>DTO de entrada para operaciones de cargo.</summary>
public sealed class DatosCargo
{
    public int?   IdCargo { get; set; }
    public string Nombre  { get; set; } = string.Empty;
    public bool   Activo  { get; set; } = true;
}
