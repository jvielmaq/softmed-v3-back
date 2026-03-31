namespace Softmed.V3.Common.Util;

using System.Collections.Concurrent;
using MySqlConnector;

/// <summary>
/// Verifica si un módulo está activo para un tenant consultando TBL_FEATURE_FLAG.
/// Resultados se cachean 5 minutos en memoria para evitar consultas repetidas por request.
/// </summary>
public static class FeatureFlag
{
    // cacheKey = "tenantId:modulo" → (activo, expiración UTC)
    private static readonly ConcurrentDictionary<string, (bool Active, DateTime ExpiresAt)> _cache = new();

    /// <summary>
    /// Indica si el módulo está activo para el tenant dado.
    /// La tabla puede tener registros globales (tenant_id = 0) que aplican a todos;
    /// un registro específico de tenant tiene precedencia sobre el global.
    /// </summary>
    /// <param name="modulo">Nombre del módulo. No puede ser vacío.</param>
    /// <param name="tenantId">Id del tenant. No puede ser 0.</param>
    /// <param name="conn">Conexión MySQL abierta.</param>
    /// <returns>true si el módulo está activo; false si no existe o está inactivo.</returns>
    /// <exception cref="ArgumentException">Si módulo es vacío o tenantId es 0.</exception>
    public static bool IsModuleActive(string modulo, int tenantId, MySqlConnection conn)
    {
        if (string.IsNullOrWhiteSpace(modulo))
            throw new ArgumentException("El nombre del módulo no puede ser vacío.", nameof(modulo));

        if (tenantId == 0)
            throw new ArgumentException("El tenantId no puede ser 0.", nameof(tenantId));

        var cacheKey = $"{tenantId}:{modulo.ToUpperInvariant()}";

        if (_cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            return cached.Active;

        // Consultar BD: el registro específico del tenant tiene precedencia
        // sobre el registro global (tenant_id = 0).
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT activo
            FROM   TBL_FEATURE_FLAG
            WHERE  modulo = @modulo
              AND  tenant_id IN (@tenantId, 0)
            ORDER BY tenant_id DESC
            LIMIT 1";

        cmd.Parameters.AddWithValue("@modulo",   modulo);
        cmd.Parameters.AddWithValue("@tenantId", tenantId);

        var result = cmd.ExecuteScalar();

        // Si no existe registro, retorna false por defecto
        bool active = result is not null && Convert.ToBoolean(result);

        _cache[cacheKey] = (active, DateTime.UtcNow.AddMinutes(5));
        return active;
    }

    /// <summary>Invalida toda la caché (útil para pruebas o cambios en caliente).</summary>
    public static void InvalidateCache() => _cache.Clear();
}
