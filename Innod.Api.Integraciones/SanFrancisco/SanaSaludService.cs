namespace Softmed.V3.Integraciones.SanFrancisco;

/// <summary>
/// Integración con Sana Salud via REST.
/// Endpoint: siliconriver.cl
/// Auth: API Key
/// Envía PDF en base64
/// Timeout: 10s
/// </summary>
public sealed class SanaSaludService
{
    private static readonly string _endpoint = Environment.GetEnvironmentVariable("SANASALUD_ENDPOINT") ?? "https://siliconriver.cl";
    private static readonly string _apiKey = Environment.GetEnvironmentVariable("SANASALUD_API_KEY") ?? "";

    public async Task<object> EnviarExamen(int idExamen, string? pdfBase64)
    {
        return new
        {
            exito = false,
            mensaje = "Integración Sana Salud pendiente de configuración.",
            endpoint = _endpoint,
            idExamen
        };
    }
}
