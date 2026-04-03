using Softmed.V3.Common.Modelo;

namespace Softmed.V3;

/// <summary>
/// Router principal: delega la ejecución al módulo correspondiente según Solicitud.MODULE.
/// </summary>
public class Accion
{
    public async Task<object> Ejecutar(Solicitud solicitud)
    {
        if (solicitud.MODULE.Equals("DIAG", StringComparison.OrdinalIgnoreCase))
        {
            await using var conn = await Common.Util.Conexion.Instance.GetConnexionAsync();
            var table = solicitud.ACTION;
            var rows = await Dapper.SqlMapper.QueryAsync(conn,
                $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t ORDER BY ORDINAL_POSITION",
                new { t = table });
            return rows;
        }

        return await (solicitud.MODULE.ToUpperInvariant() switch
        {
            "LOGIN"   => new Login.Accion().Ejecutar(solicitud),
            "CORE"    => new Core.Accion().Ejecutar(solicitud),
            "SOFTMED" => new Softmed.Accion().Ejecutar(solicitud),
            _ => throw new NotSupportedException($"Módulo '{solicitud.MODULE}' no está implementado aún.")
        });
    }
}
