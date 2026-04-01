namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaEmpleado
{
    private readonly int _tenantId;

    public LogicaEmpleado(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    // ─── ObtenerTodos ─────────────────────────────────────────────────────────

    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var lista = await RepoEmpleado.ObtenerTodos(_tenantId, conn);
        return lista;
    }

    // ─── Crear ────────────────────────────────────────────────────────────────

    public async Task<object> Crear(DatosEmpleado datos)
    {
        if (datos.IdPersona <= 0)
            throw new ArgumentException("IdPersona es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idEmpleado = await RepoEmpleado.Insertar(datos, conn);

        return new { idEmpleado, mensaje = "Empleado creado correctamente." };
    }

    // ─── CambiarEstado ────────────────────────────────────────────────────────

    public async Task<object> CambiarEstado(int idEmpleado, bool activo)
    {
        if (idEmpleado <= 0)
            throw new ArgumentException("IdEmpleado debe ser mayor a 0.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoEmpleado.CambiarEstado(idEmpleado, activo, _tenantId, conn);

        return new { mensaje = $"Empleado {(activo ? "activado" : "desactivado")} correctamente." };
    }
}
