namespace Softmed.V3.Core.Business.Logica;

using Softmed.V3.Common.Util;
using Softmed.V3.Core.Business.Repositorio;

public sealed class LogicaRol
{
    public async Task<object> ObtenerTodos()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var roles = await RepoRol.ObtenerTodos(conn);
        return roles;
    }
}
