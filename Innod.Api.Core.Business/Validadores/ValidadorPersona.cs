namespace Softmed.V3.Core.Business.Validadores;

using Softmed.V3.Core.Business.Modelo;

public static class ValidadorPersona
{
    /// <exception cref="ArgumentException">Con mensaje descriptivo si falla alguna regla.</exception>
    public static void Validar(DatosPersona datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombres))
            throw new ArgumentException("Nombres es requerido.");

        if (string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ArgumentException("Apellidos es requerido.");

        if (string.IsNullOrWhiteSpace(datos.Identificador))
            throw new ArgumentException("Identificador es requerido.");

        if (datos.IdTipoIdentificador <= 0)
            throw new ArgumentException("IdTipoIdentificador es requerido y debe ser mayor a 0.");
    }
}
