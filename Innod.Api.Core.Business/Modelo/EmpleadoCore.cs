namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Proyección de salida para empleado.</summary>
public sealed class EmpleadoCore
{
    public int    IdEmpleado    { get; set; }
    public int    IdPersona     { get; set; }
    public string Nombres       { get; set; } = string.Empty;
    public string Apellidos     { get; set; } = string.Empty;
    public string Identificador { get; set; } = string.Empty;
    public int    IdCargo       { get; set; }
    public string Cargo         { get; set; } = string.Empty;
    public int    IdEspecialidad { get; set; }
    public string Especialidad  { get; set; } = string.Empty;
    public bool   Activo        { get; set; }
}
