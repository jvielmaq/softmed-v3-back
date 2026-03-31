using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Softmed.V3.Common.Modelo;
using Softmed.V3.Common.Util;

namespace Softmed.V3;

/// <summary>
/// Entry point AWS Lambda.
/// Flujo:
///   1. Deserializa el body en <see cref="Solicitud"/>.
///   2. Extrae la IP del contexto API Gateway.
///   3. Si MODULE != "LOGIN", valida el JWT y extrae TenantId.
///   4. Delega a <see cref="Accion.Ejecutar"/>.
///   5. Maneja excepciones:
///      - <see cref="UnauthorizedAccessException"/> → 401
///      - <see cref="FeatureFlagException"/>        → 403 "Módulo no disponible"
///      - cualquier otra                            → 500 (log en CloudWatch)
/// </summary>
public class Function
{
    private static readonly Token _tokenService = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        Solicitud? solicitud = null;

        try
        {
            // ── 1. Parsear body ───────────────────────────────────────────────
            solicitud = JsonSerializer.Deserialize<Solicitud>(
                            request.Body ?? "{}",
                            _jsonOptions);

            if (solicitud is null || string.IsNullOrWhiteSpace(solicitud.MODULE))
                return BuildResponse(400, new { error = "Solicitud inválida o MODULE ausente." });

            // ── 2. Extraer IP ─────────────────────────────────────────────────
            solicitud.Ip = request.RequestContext?.Identity?.SourceIp ?? string.Empty;

            // ── 3. Validar JWT (omitir para módulo LOGIN) ─────────────────────
            if (!solicitud.MODULE.Equals("LOGIN", StringComparison.OrdinalIgnoreCase))
            {
                var userData = _tokenService.ValidaToken(solicitud.Token ?? string.Empty, solicitud.Ip);

                if (userData is null)
                    throw new UnauthorizedAccessException("Token inválido, expirado o IP no coincide.");

                solicitud.TenantId = userData.TenantId;
                solicitud.UserData = userData;
            }

            // ── 4. Ejecutar acción ────────────────────────────────────────────
            var resultado = await new Accion().Ejecutar(solicitud);

            return BuildResponse(200, resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Logger.LogError($"[401] {ex.Message}");
            return BuildResponse(401, new { error = ex.Message });
        }
        catch (FeatureFlagException ex)
        {
            context.Logger.LogWarning($"[403] Módulo bloqueado por FeatureFlag: {ex.Modulo}");
            return BuildResponse(403, new { error = "Módulo no disponible." });
        }
        catch (Exception ex)
        {
            var isDev = Environment.GetEnvironmentVariable("AMBIENTE") == "DEV";
            context.Logger.LogError($"[500] {ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message}");
            return BuildResponse(500, isDev
                ? new { error = ex.Message, tipo = ex.GetType().Name, inner = ex.InnerException?.Message }
                : (object)new { error = "Error interno del servidor." });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static APIGatewayProxyResponse BuildResponse(int statusCode, object body) =>
        new()
        {
            StatusCode = statusCode,
            Body       = JsonSerializer.Serialize(body),
            Headers    = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            }
        };
}
