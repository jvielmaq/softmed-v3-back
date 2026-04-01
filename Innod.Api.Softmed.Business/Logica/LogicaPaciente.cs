using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Softmed.V3.Softmed.Business.Modelo;
using Softmed.V3.Softmed.Business.Repositorio;

namespace Softmed.V3.Softmed.Business.Logica;

public sealed class LogicaPaciente
{
    private readonly int _tenantId;

    public LogicaPaciente(Solicitud solicitud)
    {
        _tenantId = solicitud.TenantId;
    }

    public async Task<object> ListaPacientes(FiltrosPaciente filtros)
    {
        if (filtros.RegistrosPorPagina <= 0) filtros.RegistrosPorPagina = 50;
        if (filtros.Pagina <= 0)             filtros.Pagina = 1;

        await using var conn = await Conexion.Instance.GetConnexionAsync();

        var total = await RepoPaciente.ContarPacientes(filtros, _tenantId, conn);
        var datos = await RepoPaciente.ObtenerPacientes(filtros, _tenantId, conn);

        return new { total, pagina = filtros.Pagina, datos };
    }
}
