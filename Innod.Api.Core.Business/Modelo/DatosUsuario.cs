namespace Softmed.V3.Core.Business.Modelo;

/// <summary>
/// DTO de entrada unificado para todas las operaciones de usuario en módulo CORE.
/// Los campos utilizados varían según el TARGET:
///   CREAR / ACTUALIZAR → Nombres, Apellidos, Email, Password, IdRol, IdInstitucion, IdUsuario (ACTUALIZAR)
///   CAMBIAR_CLAVE      → IdUsuario, ClaveActual, ClaveNueva
///   CAMBIAR_ESTADO     → IdUsuario, Activo
///   OBTENER_POR_ID     → IdUsuario
/// </summary>
public sealed class DatosUsuario
{
    public int?   IdUsuario     { get; set; }
    public string Nombres       { get; set; } = string.Empty;
    public string Apellidos     { get; set; } = string.Empty;
    public string Email         { get; set; } = string.Empty;
    public string Password      { get; set; } = string.Empty;
    public int    IdRol         { get; set; }
    public int    IdInstitucion { get; set; }
    public string ClaveActual   { get; set; } = string.Empty;
    public string ClaveNueva    { get; set; } = string.Empty;
    public bool   Activo        { get; set; }
}
