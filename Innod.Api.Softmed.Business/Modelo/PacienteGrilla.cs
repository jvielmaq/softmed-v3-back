namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Proyección de paciente para listado en Softmed.</summary>
public sealed class PacienteGrilla
{
    public int      IdPersona       { get; set; }
    public string   Nombres         { get; set; } = string.Empty;
    public string   Apellidos       { get; set; } = string.Empty;
    public string   Identificador   { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public int      TotalExamenes   { get; set; }
}
