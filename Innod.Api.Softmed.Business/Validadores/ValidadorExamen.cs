using Softmed.V3.Softmed.Business.Modelo;

namespace Softmed.V3.Softmed.Business.Validadores;

public static class ValidadorExamen
{
    /// <exception cref="ArgumentException">Con mensaje descriptivo si falla alguna regla.</exception>
    public static void Validar(DatosCrearExamen datos)
    {
        if (datos.PacienteId <= 0)
            throw new ArgumentException("PacienteId es requerido y debe ser mayor a 0.");

        if (datos.InstitucionId <= 0)
            throw new ArgumentException("InstitucionId es requerido y debe ser mayor a 0.");

        if (datos.TipoSolicitudId <= 0)
            throw new ArgumentException("TipoSolicitudId es requerido y debe ser mayor a 0.");

        if (datos.FechaMuestra == default)
            throw new ArgumentException("FechaMuestra es requerida.");

        if (datos.FechaMuestra.Date > DateTime.UtcNow.Date)
            throw new ArgumentException(
                $"FechaMuestra ({datos.FechaMuestra:yyyy-MM-dd}) no puede ser una fecha futura.");
    }
}
