namespace Softmed.V3.Common.Util;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Utilidades de seguridad: hashing de contraseñas y generación de claves.
/// </summary>
public static class Seguridad
{
    /// <summary>
    /// Verifica si <paramref name="passwordIngresado"/> coincide con <paramref name="hashGuardado"/>.
    /// Algoritmo: PBKDF2-HMAC-SHA1, 1000 iteraciones, 32 bytes de salida.
    /// Salt: MD5(<paramref name="userKey"/>) en hex lowercase → bytes ASCII.
    /// El hash guardado se espera en Base64.
    /// </summary>
    public static bool VerificaPassword(string passwordIngresado, string hashGuardado, string userKey)
    {
        if (string.IsNullOrEmpty(passwordIngresado) ||
            string.IsNullOrEmpty(hashGuardado)       ||
            string.IsNullOrEmpty(userKey))
            return false;

        // Salt: MD5(userKey) → hex lowercase → ASCII bytes
        var md5Hash  = MD5.HashData(Encoding.UTF8.GetBytes(userKey));
        var saltHex  = Convert.ToHexString(md5Hash).ToLowerInvariant();   // 32 hex chars
        var salt     = Encoding.ASCII.GetBytes(saltHex);

        // PBKDF2-HMAC-SHA1, 1000 iter, 32 bytes
        var derived  = Rfc2898DeriveBytes.Pbkdf2(
            password:      Encoding.UTF8.GetBytes(passwordIngresado),
            salt:          salt,
            iterations:    1000,
            hashAlgorithm: HashAlgorithmName.SHA1,
            outputLength:  32);

        byte[] stored;
        try   { stored = Convert.FromBase64String(hashGuardado); }
        catch { return false; }

        // Comparación en tiempo constante para evitar timing attacks
        return CryptographicOperations.FixedTimeEquals(derived, stored);
    }

    /// <summary>
    /// Genera una clave aleatoria alfanumérica de <paramref name="length"/> caracteres (default 6).
    /// Usa <see cref="RandomNumberGenerator"/> para garantizar aleatoriedad criptográfica.
    /// </summary>
    public static string GeneraRandomKey(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var bytes  = RandomNumberGenerator.GetBytes(length);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    /// <summary>
    /// Genera el hash PBKDF2-HMAC-SHA1 de una contraseña usando el mismo algoritmo
    /// que <see cref="VerificaPassword"/>.  Retorna el hash en Base64.
    /// </summary>
    public static string HashPassword(string password, string userKey)
    {
        var md5Hash = MD5.HashData(Encoding.UTF8.GetBytes(userKey));
        var saltHex = Convert.ToHexString(md5Hash).ToLowerInvariant();
        var salt    = Encoding.ASCII.GetBytes(saltHex);

        var derived = Rfc2898DeriveBytes.Pbkdf2(
            password:      Encoding.UTF8.GetBytes(password),
            salt:          salt,
            iterations:    1000,
            hashAlgorithm: HashAlgorithmName.SHA1,
            outputLength:  32);

        return Convert.ToBase64String(derived);
    }
}
