using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Modelo;
using Softmed.V3.Softmed.Business.Repositorio;

namespace Softmed.V3.Softmed.Business.Logica;

public sealed class LogicaSedePabellon
{
    private readonly int _tenantId;

    public LogicaSedePabellon(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SEDE
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<object> ListaSedes()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoSedePabellon.ListaSedes(_tenantId, conn);
    }

    public async Task<object> ObtieneSede(int idSede)
    {
        if (idSede <= 0) throw new ArgumentException("IdSede debe ser mayor a 0.");
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var sede = await RepoSedePabellon.ObtieneSede(idSede, _tenantId, conn);
        return sede ?? throw new KeyNotFoundException(
            $"Sede {idSede} no encontrada en este tenant.");
    }

    public async Task<object> CrearSede(DatosSede datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        if (datos.IdInstitucion <= 0)
            throw new ArgumentException("IdInstitucion es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idSede = await RepoSedePabellon.InsertaSede(datos, conn);

        return new { idSede, mensaje = "Sede creada correctamente." };
    }

    public async Task<object> ActualizarSede(DatosSede datos)
    {
        if (datos.IdSede is null or <= 0)
            throw new ArgumentException("IdSede es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoSedePabellon.ActualizaSede(datos, _tenantId, conn);

        return new { mensaje = "Sede actualizada correctamente." };
    }

    public async Task<object> EliminarSede(int idSede)
    {
        if (idSede <= 0) throw new ArgumentException("IdSede debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoSedePabellon.EliminaSede(idSede, _tenantId, conn);

        return new { mensaje = "Sede eliminada correctamente." };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PABELLON
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<object> ListaPabellones()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await RepoSedePabellon.ListaPabellones(_tenantId, conn);
    }

    public async Task<object> CrearPabellon(DatosPabellon datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");
        if (datos.IdSede <= 0)
            throw new ArgumentException("IdSede es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idPabellon = await RepoSedePabellon.InsertaPabellon(datos, conn);

        return new { idPabellon, mensaje = "Pabellón creado correctamente." };
    }

    public async Task<object> ActualizarPabellon(DatosPabellon datos)
    {
        if (datos.IdPabellon is null or <= 0)
            throw new ArgumentException("IdPabellon es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoSedePabellon.ActualizaPabellon(datos, _tenantId, conn);

        return new { mensaje = "Pabellón actualizado correctamente." };
    }
}
