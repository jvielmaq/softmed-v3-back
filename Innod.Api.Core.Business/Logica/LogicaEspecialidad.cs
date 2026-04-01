namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaEspecialidad
{
    private readonly int _tenantId;

    public LogicaEspecialidad(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var lista = await RepoEspecialidad.ObtenerTodos(_tenantId, conn);
        return lista;
    }

    public async Task<object> Crear(DatosEspecialidad datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idEspecialidad = await RepoEspecialidad.Insertar(datos, conn);

        return new { idEspecialidad, mensaje = "Especialidad creada correctamente." };
    }

    public async Task<object> Actualizar(DatosEspecialidad datos)
    {
        if (datos.IdEspecialidad is null or <= 0)
            throw new ArgumentException("IdEspecialidad es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoEspecialidad.Actualizar(datos, _tenantId, conn);

        return new { mensaje = "Especialidad actualizada correctamente." };
    }
}
