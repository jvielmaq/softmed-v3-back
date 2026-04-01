namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Modelo;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaCargo
{
    private readonly int _tenantId;

    public LogicaCargo(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var lista = await RepoCargo.ObtenerTodos(_tenantId, conn);
        return lista;
    }

    public async Task<object> Crear(DatosCargo datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var idCargo = await RepoCargo.Insertar(datos, conn);

        return new { idCargo, mensaje = "Cargo creado correctamente." };
    }

    public async Task<object> Actualizar(DatosCargo datos)
    {
        if (datos.IdCargo is null or <= 0)
            throw new ArgumentException("IdCargo es requerido para actualizar.");
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre es requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        await RepoCargo.Actualizar(datos, _tenantId, conn);

        return new { mensaje = "Cargo actualizado correctamente." };
    }
}
