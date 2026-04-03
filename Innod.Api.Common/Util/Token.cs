namespace Softmed.V3.Common.Util;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

/// <summary>Datos del usuario embebidos en el claim "UserData" del JWT.</summary>
public sealed class UserData
{
    public int    IdUsuario   { get; set; }
    public int    TenantId    { get; set; }
    public string[] Roles     { get; set; } = [];
    public string Ip          { get; set; } = string.Empty;
    public string TipoOrigen  { get; set; } = string.Empty;
}

/// <summary>
/// Genera y valida JWT (HMAC-SHA256).
/// Configuración desde variables de entorno:
///   JWT_SECRET   → clave de firma  (requerida)
///   JWT_ISSUER   → issuer          (opcional, default "softmed")
///   JWT_AUDIENCE → audience        (opcional, default "softmed")
/// Expiración: 60 min para usuarios normales.
/// Refresh token: 7 días, almacenado en TBL_REFRESH_TOKEN.
/// </summary>
public sealed class Token
{
    private static readonly string _secret =
        Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("Variable de entorno JWT_SECRET no definida.");

    private static readonly string _issuer   = Environment.GetEnvironmentVariable("JWT_ISSUER")   ?? "softmed";
    private static readonly string _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "softmed";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    // ─── Generación ────────────────────────────────────────────────────────────

    /// <summary>Genera un JWT firmado con los datos del usuario. Expira en 60 minutos.</summary>
    public string GeneraToken(UserData userData)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserData", JsonSerializer.Serialize(userData, _jsonOptions)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                      DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                      ClaimValueTypes.Integer64)
        };

        var jwtToken = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    /// <summary>
    /// Genera un refresh token opaco (64 bytes base64) y lo persiste en TBL_REFRESH_TOKEN.
    /// Expira en 7 días.
    /// </summary>
    public async Task<string> GeneraRefreshTokenAsync(UserData userData, MySqlConnection conn)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiry       = DateTime.UtcNow.AddDays(7);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO TBL_REFRESH_TOKEN
                (token, id_usuario, tenant_id, expiry, ip, created_at)
            VALUES
                (@token, @idUsuario, @tenantId, @expiry, @ip, UTC_TIMESTAMP())";

        cmd.Parameters.AddWithValue("@token",     refreshToken);
        cmd.Parameters.AddWithValue("@idUsuario", userData.IdUsuario);
        cmd.Parameters.AddWithValue("@tenantId",  userData.TenantId);
        cmd.Parameters.AddWithValue("@expiry",    expiry);
        cmd.Parameters.AddWithValue("@ip",        userData.Ip);

        await cmd.ExecuteNonQueryAsync();
        return refreshToken;
    }

    // ─── Validación ────────────────────────────────────────────────────────────

    /// <summary>
    /// Valida firma, expiración e IP del token.
    /// Retorna <see cref="UserData"/> si es válido; null si no lo es.
    /// </summary>
    public UserData? ValidaToken(string tokenString, string ip)
    {
        if (string.IsNullOrWhiteSpace(tokenString))
            return null;

        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(tokenString, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _issuer,
                ValidateAudience         = true,
                ValidAudience            = _audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero       // sin tolerancia
            }, out var validatedToken);

            var jwt           = (JwtSecurityToken)validatedToken;
            var userDataJson  = jwt.Claims.FirstOrDefault(c => c.Type == "UserData")?.Value;

            if (string.IsNullOrWhiteSpace(userDataJson))
                return null;

            var userData = JsonSerializer.Deserialize<UserData>(userDataJson, _jsonOptions);
            if (userData is null)
                return null;

            // Verificación de IP (deshabilitada en DEV para permitir acceso desde browser)
            var ambiente = Environment.GetEnvironmentVariable("AMBIENTE") ?? "";
            if (ambiente != "DEV" && !string.IsNullOrEmpty(ip) && userData.Ip != ip)
                return null;

            return userData;
        }
        catch
        {
            return null;
        }
    }
}
