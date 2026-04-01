namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaPersona
{
    private readonly int _tenantId;

    public LogicaPersona(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ─── ObtenerTodos ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerTodos(FiltrosPersona filtros)
    {
        if (filtros.RegistrosPorPagina <= 0) filtros.RegistrosPorPagina = 50;
        if (filtros.Pagina <= 0)             filtros.Pagina = 1;

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var total = await RepoPersona.ContarTodos(_tenantId, conn);
        var datos = await RepoPersona.ObtenerTodos(
            _tenantId, filtros.RegistrosPorPagina, filtros.Offset, conn);

        return new { total, pagina = filtros.Pagina, datos };
    }

    // ─── ObtenerPorId ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerPorId(int idPersona)
    {
        if (idPersona <= 0)
            throw new ArgumentException("IdPersona debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var persona = await RepoPersona.ObtenerPorId(idPersona, _tenantId, conn);

        if (persona is null)
            throw new KeyNotFoundException(
                $"Persona {idPersona} no encontrada en este tenant.");

        return persona;
    }

    // ─── Crear ────────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosPersona datos)
    {
        ValidarDatosCrear(datos);

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idPersona = await RepoPersona.Insertar(datos, conn);

        return new { idPersona, mensaje = "Persona creada correctamente." };
    }

    // ─── Actualizar ───────────────────────────────────────────────────────────

    public async Task<object> Actualizar(DatosPersona datos)
    {
        if (datos.IdPersona is null or <= 0)
            throw new ArgumentException("IdPersona es requerido para actualizar.");
        ValidarDatosCrear(datos);

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var existente = await RepoPersona.ObtenerPorId(datos.IdPersona.Value, _tenantId, conn);
        if (existente is null)
            throw new KeyNotFoundException(
                $"Persona {datos.IdPersona} no encontrada en este tenant.");

        await RepoPersona.Actualizar(datos, _tenantId, conn);

        return new { mensaje = "Persona actualizada correctamente." };
    }

    // ─── CambiarEstado ────────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(int idPersona, bool activo)
    {
        if (idPersona <= 0)
            throw new ArgumentException("IdPersona debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoPersona.CambiarEstado(idPersona, activo, _tenantId, conn);

        return new { mensaje = $"Persona {(activo ? "activada" : "desactivada")} correctamente." };
    }

    // ─── Buscar ───────────────────────────────────────────────────────────────

    public async Task<object> Buscar(FiltrosPersona filtros)
    {
        if (filtros.RegistrosPorPagina <= 0) filtros.RegistrosPorPagina = 50;
        if (filtros.Pagina <= 0)             filtros.Pagina = 1;

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var datos = await RepoPersona.Buscar(filtros, _tenantId, conn);

        return datos;
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private static void ValidarDatosCrear(DatosPersona datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombres))
            throw new ArgumentException("Nombres es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Apellidos))
            throw new ArgumentException("Apellidos es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Identificador))
            throw new ArgumentException("Identificador es requerido.");
        if (datos.IdTipoIdentificador <= 0)
            throw new ArgumentException("IdTipoIdentificador es requerido.");
    }
}
