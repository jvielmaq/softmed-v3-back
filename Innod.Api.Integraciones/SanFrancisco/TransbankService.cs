namespace Softmed.V3.Integraciones.SanFrancisco;

/// <summary>
/// Integración con Transbank Webpay Plus.
/// Variables: TBK_URL, TBK_KEY, TBK_SECRET
/// Timeout: 30s
/// </summary>
public sealed class TransbankService
{
    private static readonly string _url = Environment.GetEnvironmentVariable("TBK_URL") ?? "";
    private static readonly string _key = Environment.GetEnvironmentVariable("TBK_KEY") ?? "";
    private static readonly string _secret = Environment.GetEnvironmentVariable("TBK_SECRET") ?? "";

    public async Task<object> IniciarPago(decimal monto, string ordenCompra, string returnUrl)
    {
        return new
        {
            exito = false,
            mensaje = "Integración Transbank pendiente de configuración.",
            ordenCompra
        };
    }

    public async Task<object> ConfirmarPago(string tokenWs)
    {
        return new
        {
            exito = false,
            mensaje = "Integración Transbank pendiente de configuración.",
            tokenWs
        };
    }
}
