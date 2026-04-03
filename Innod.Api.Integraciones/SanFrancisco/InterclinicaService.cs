namespace Softmed.V3.Integraciones.SanFrancisco;

/// <summary>
/// Integración con Interclinica via SOAP XML.
/// Endpoint: 172.16.1.233
/// Auth: usuario/clave en envelope SOAP
/// Timeout: 15s
/// </summary>
public sealed class InterclinicaService
{
    private static readonly string _endpoint = Environment.GetEnvironmentVariable("INTERCLINICA_ENDPOINT") ?? "http://172.16.1.233";
    private static readonly string _user = Environment.GetEnvironmentVariable("INTERCLINICA_USER") ?? "";
    private static readonly string _pass = Environment.GetEnvironmentVariable("INTERCLINICA_PASS") ?? "";

    public async Task<object> EnviarExamen(int idExamen, object examenData)
    {
        return new
        {
            exito = false,
            mensaje = "Integración Interclinica pendiente de configuración.",
            endpoint = _endpoint,
            idExamen
        };
    }
}
