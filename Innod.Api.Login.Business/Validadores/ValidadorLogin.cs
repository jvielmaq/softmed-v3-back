namespace Softmed.V3.Login.Business.Validadores;

using System.Text.RegularExpressions;

/// <summary>Valida las entradas de la acción AUTENTICAR antes de tocar la BD.</summary>
public static class ValidadorLogin
{
    // RFC 5322 simplificado — suficiente para validación de entrada
    private static readonly Regex _emailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Valida email y password.
    /// </summary>
    /// <exception cref="ArgumentException">Si algún campo es inválido, con mensaje descriptivo.</exception>
    public static void Validar(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es requerido.");

        if (!_emailRegex.IsMatch(email))
            throw new ArgumentException($"El email '{email}' no tiene un formato válido.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña es requerida.");

        if (password.Length < 6)
            throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");
    }
}
