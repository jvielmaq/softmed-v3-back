namespace Softmed.V3.Login.Business.Modelo;

/// <summary>Datos del usuario tal como se recuperan de la base de datos.</summary>
public sealed class UsuarioLogin
{
    public int    IdUsuario    { get; set; }
    public string Nombres      { get; set; } = string.Empty;
    public string Apellidos    { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string Clave        { get; set; } = string.Empty;   // hash almacenado
    public string Key          { get; set; } = string.Empty;   // salt derivado del usuario
    public string NombreRol    { get; set; } = string.Empty;
    public bool   Activo          { get; set; }
    public bool   RequiereCambio  { get; set; }
    public int    IdInstitucion   { get; set; }
    public int    TenantId        { get; set; }
}
