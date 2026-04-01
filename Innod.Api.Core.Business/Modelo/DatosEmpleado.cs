namespace Softmed.V3.Core.Business.Modelo;

/// <summary>DTO de entrada para operaciones de empleado.</summary>
public sealed class DatosEmpleado
{
    public int? IdEmpleado    { get; set; }
    public int  IdPersona     { get; set; }
    public int  IdCargo       { get; set; }
    public int  IdEspecialidad { get; set; }
    public bool Activo        { get; set; } = true;
}
