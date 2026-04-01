using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Modelo;
using Softmed.V3.Softmed.Business.Repositorio;

namespace Softmed.V3.Softmed.Business.Logica;

public sealed class LogicaOperacionInterna
{
    private readonly int _tenantId;

    public LogicaOperacionInterna(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIPO MUESTRA
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<object> TipoMuestraLista()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoOperacionInterna.TipoMuestraLista(_tenantId, conn);
    }

    public async Task<object> TipoMuestraObtiene(int id)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var item = await RepoOperacionInterna.TipoMuestraObtiene(id, _tenantId, conn);
        return item ?? throw new KeyNotFoundException(
            $"TipoMuestra {id} no encontrado en este tenant.");
    }

    public async Task<object> TipoMuestraNuevo(DatosTipoMuestra datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await RepoOperacionInterna.TipoMuestraInserta(datos, conn);
        return new { id, mensaje = "Tipo de muestra creado correctamente." };
    }

    public async Task<object> TipoMuestraEditar(DatosTipoMuestra datos)
    {
        if (datos.IdTipoMuestra is null or <= 0)
            throw new ArgumentException("IdTipoMuestra es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.TipoMuestraActualiza(datos, _tenantId, conn);
        return new { mensaje = "Tipo de muestra actualizado correctamente." };
    }

    public async Task<object> TipoMuestraCambiaEstado(int id, bool activo)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.TipoMuestraCambiaEstado(id, activo, _tenantId, conn);
        return new { mensaje = $"Tipo de muestra {(activo ? "activado" : "desactivado")} correctamente." };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ORGANO
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<object> OrganoLista()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoOperacionInterna.OrganoLista(_tenantId, conn);
    }

    public async Task<object> OrganoObtiene(int id)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var item = await RepoOperacionInterna.OrganoObtiene(id, _tenantId, conn);
        return item ?? throw new KeyNotFoundException(
            $"Órgano {id} no encontrado en este tenant.");
    }

    public async Task<object> OrganoNuevo(DatosOrgano datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await RepoOperacionInterna.OrganoInserta(datos, conn);
        return new { id, mensaje = "Órgano creado correctamente." };
    }

    public async Task<object> OrganoEditar(DatosOrgano datos)
    {
        if (datos.IdOrgano is null or <= 0)
            throw new ArgumentException("IdOrgano es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.OrganoActualiza(datos, _tenantId, conn);
        return new { mensaje = "Órgano actualizado correctamente." };
    }

    public async Task<object> OrganoCambiaEstado(int id, bool activo)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.OrganoCambiaEstado(id, activo, _tenantId, conn);
        return new { mensaje = $"Órgano {(activo ? "activado" : "desactivado")} correctamente." };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIPO SOLICITUD
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<object> TipoSolicitudLista()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoOperacionInterna.TipoSolicitudLista(_tenantId, conn);
    }

    public async Task<object> TipoSolicitudObtiene(int id)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var item = await RepoOperacionInterna.TipoSolicitudObtiene(id, _tenantId, conn);
        return item ?? throw new KeyNotFoundException(
            $"TipoSolicitud {id} no encontrado en este tenant.");
    }

    public async Task<object> TipoSolicitudNuevo(DatosTipoSolicitud datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await RepoOperacionInterna.TipoSolicitudInserta(datos, conn);
        return new { id, mensaje = "Tipo de solicitud creado correctamente." };
    }

    public async Task<object> TipoSolicitudEditar(DatosTipoSolicitud datos)
    {
        if (datos.IdTipoSolicitud is null or <= 0)
            throw new ArgumentException("IdTipoSolicitud es requerido.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.TipoSolicitudActualiza(datos, _tenantId, conn);
        return new { mensaje = "Tipo de solicitud actualizado correctamente." };
    }

    public async Task<object> TipoSolicitudCambiaEstado(int id, bool activo)
    {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoOperacionInterna.TipoSolicitudCambiaEstado(id, activo, _tenantId, conn);
        return new { mensaje = $"Tipo de solicitud {(activo ? "activado" : "desactivado")} correctamente." };
    }
}
