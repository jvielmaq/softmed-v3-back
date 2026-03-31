namespace Softmed.V3.Core.Business.Modelo;

public sealed class UsuarioCore
{
    public int    IdUsuario     { get; set; }
    public string Nombres       { get; set; } = string.Empty;
    public string Apellidos     { get; set; } = string.Empty;
    public string Email         { get; set; } = string.Empty;
    public string NombreRol     { get; set; } = string.Empty;
    public bool   Activo        { get; set; }
    public int    IdInstitucion { get; set; }
}
