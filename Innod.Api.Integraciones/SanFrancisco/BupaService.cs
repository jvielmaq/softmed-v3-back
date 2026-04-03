namespace Softmed.V3.Integraciones.SanFrancisco;

/// <summary>
/// Integración con BUPA via REST HMAC-SHA256.
/// Endpoint: api.bupa.cl
/// Headers: x-kong-source, Date, Authorization
/// Timeout: 10s
/// </summary>
public sealed class BupaService
{
    private static readonly string _endpoint = Environment.GetEnvironmentVariable("BUPA_ENDPOINT") ?? "https://api.bupa.cl";
    private static readonly string _apiKey = Environment.GetEnvironmentVariable("BUPA_API_KEY") ?? "";
    private static readonly string _secret = Environment.GetEnvironmentVariable("BUPA_SECRET") ?? "";

    public async Task<object> EnviarExamen(int idExamen, object examenData)
    {
        // TODO: Implementar HMAC-SHA256 signing y envío real
        // Por ahora retorna stub
        return new
        {
            exito = false,
            mensaje = "Integración BUPA pendiente de configuración.",
            endpoint = _endpoint,
            idExamen
        };
    }
}
