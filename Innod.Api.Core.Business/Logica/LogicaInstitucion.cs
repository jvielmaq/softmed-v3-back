namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaInstitucion
{
    private readonly int _tenantId;

    public LogicaInstitucion(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ─── ObtenerTodos ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var lista = await RepoInstitucion.ObtenerTodos(_tenantId, conn);
        return lista;
    }

    // ─── ObtenerPorId ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerPorId(int idInstitucion)
    {
        if (idInstitucion <= 0)
            throw new ArgumentException("IdInstitucion debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var institucion = await RepoInstitucion.ObtenerPorId(
            idInstitucion, _tenantId, conn);

        if (institucion is null)
            throw new KeyNotFoundException(
                $"Institución {idInstitucion} no encontrada en este tenant.");

        return institucion;
    }

    // ─── Crear ────────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosInstitucion datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idInstitucion = await RepoInstitucion.Insertar(datos, conn);

        return new { idInstitucion, mensaje = "Institución creada correctamente." };
    }

    // ─── Actualizar ───────────────────────────────────────────────────────────

    public async Task<object> Actualizar(DatosInstitucion datos)
    {
        if (datos.IdInstitucion is null or <= 0)
            throw new ArgumentException("IdInstitucion es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var existente = await RepoInstitucion.ObtenerPorId(
            datos.IdInstitucion.Value, _tenantId, conn);
        if (existente is null)
            throw new KeyNotFoundException(
                $"Institución {datos.IdInstitucion} no encontrada en este tenant.");

        await RepoInstitucion.Actualizar(datos, _tenantId, conn);

        return new { mensaje = "Institución actualizada correctamente." };
    }

    // ─── CambiarEstado ────────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(int idInstitucion, bool activo)
    {
        if (idInstitucion <= 0)
            throw new ArgumentException("IdInstitucion debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoInstitucion.CambiarEstado(idInstitucion, activo, _tenantId, conn);

        return new { mensaje = $"Institución {(activo ? "activada" : "desactivada")} correctamente." };
    }
}
