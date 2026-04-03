using System.Text.Json;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;
using Dapper;

namespace Softmed.V3.Softmed.Interfaz;

internal sealed class Laboratorio
{
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };
    private readonly Solicitud _solicitud;

    public Laboratorio(Solicitud solicitud) => _solicitud = solicitud;

    public async Task<object> Ejecutar()
    {
        return _solicitud.TARGET.ToUpperInvariant() switch
        {
            "LISTA_LABORATORIOS" => await ListaLaboratorios(),
            "CREAR"              => await Crear(),
            _ => throw new ArgumentException($"TARGET '{_solicitud.TARGET}' no reconocido en LABORATORIO.")
        };
    }

    private async Task<object> ListaLaboratorios()
    {
        await using var conn = await Conexion.Instance.GetConnexionAsync();
        return await conn.QueryAsync(@"
            SELECT l.id_laboratorio AS IdLaboratorio, l.nombre AS Nombre,
                   i.nombre AS Institucion, l.activo AS Activo
            FROM TBL_LABORATORIO l
            LEFT JOIN TBL_INSTITUCION i ON i.id_institucion = l.id_institucion
            INNER JOIN TBL_TENANT t ON t.id_institucion = COALESCE(l.id_institucion, t.id_institucion)
            WHERE t.id_tenant = @tenantId AND l.activo = 1
            ORDER BY l.nombre", new { tenantId = _solicitud.TenantId });
    }

    private async Task<object> Crear()
    {
        if (_solicitud.Data is not JsonElement el) throw new ArgumentException("Data requerido.");
        var datos = el.Deserialize<DatosLab>(_jsonOpts) ?? throw new ArgumentException("Data requerido.");
        if (string.IsNullOrWhiteSpace(datos.Nombre)) throw new ArgumentException("Nombre requerido.");

        await using var conn = await Conexion.Instance.GetConnexionAsync();
        var id = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO TBL_LABORATORIO (nombre, id_institucion, activo) VALUES (@Nombre, @IdInstitucion, 1); SELECT LAST_INSERT_ID();",
            datos);
        return new { idLaboratorio = id, mensaje = "Laboratorio creado." };
    }

    private sealed class DatosLab { public string Nombre { get; set; } = ""; public int? IdInstitucion { get; set; } }
}
