namespace Softmed.V3.Common.Util;

/// <summary>
/// Excepción lanzada cuando un módulo no está disponible para el tenant
/// según las reglas de TBL_FEATURE_FLAG.
/// </summary>
public sealed class FeatureFlagException : Exception
{
    public string Modulo { get; }

    public FeatureFlagException(string modulo)
        : base($"El módulo '{modulo}' no está disponible para este tenant.")
    {
        Modulo = modulo;
    }

    public FeatureFlagException(string modulo, string message)
        : base(message)
    {
        Modulo = modulo;
    }
}
