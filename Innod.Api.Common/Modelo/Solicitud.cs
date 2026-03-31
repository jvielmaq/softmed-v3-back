namespace Softmed.V3.Common.Modelo;

using Softmed.V3.Common.Util;

/// <summary>
/// Representa el cuerpo de cada request que llega a la Lambda.
/// MODULE   → identifica el módulo destino (ej. LOGIN, SOFTMED, CORE).
/// ACTION   → operación a ejecutar dentro del módulo.
/// TARGET   → recurso o entidad sobre la que se opera.
/// Token    → JWT del usuario autenticado (vacío para módulo LOGIN).
/// Ip       → IP del cliente, extraída por Function.cs del contexto Lambda.
/// TenantId → extraído del token por Function.cs; 0 hasta que se valide.
/// UserData → objeto UserData completo, propagado por Function.cs tras validar el JWT.
/// </summary>
public sealed class Solicitud
{
    public string  MODULE    { get; set; } = string.Empty;
    public string  ACTION    { get; set; } = string.Empty;
    public string  TARGET    { get; set; } = string.Empty;
    public string? Token     { get; set; }
    public string  Ip        { get; set; } = string.Empty;
    public int     TenantId  { get; set; }
    public UserData? UserData { get; set; }

    /// <summary>Payload adicional libre para los módulos.</summary>
    public object? Data { get; set; }
}
