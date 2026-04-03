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

    private static readonly JsonSerializerOptions _jsonOutputOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        // ── 0. Preflight CORS ────────────────────────────────────────────
        var httpMethod = request.HttpMethod ?? request.RequestContext?.HttpMethod ?? "";
        if (httpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body       = string.Empty,
                Headers    = new Dictionary<string, string>
                {
                    ["Access-Control-Allow-Origin"]  = "*",
                    ["Access-Control-Allow-Headers"] = "Content-Type,Authorization",
                    ["Access-Control-Allow-Methods"] = "POST,OPTIONS"
                }
            };
        }

        Solicitud? solicitud = null;

        try
        {
            // ── 1. Parsear body ───────────────────────────────────────────────
            var body = request.Body ?? string.Empty;
            solicitud = new Solicitud();
            foreach (var pair in body.Split('&'))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length != 2) continue;
                var key = Uri.UnescapeDataString(parts[0].Trim());
                var val = Uri.UnescapeDataString(parts[1].Trim());
                switch (key.ToUpper())
                {
                    case "MODULE": solicitud.MODULE = val; break;
                    case "ACTION": solicitud.ACTION = val; break;
                    case "TARGET": solicitud.TARGET = val; break;
                    case "TOKEN":  solicitud.Token  = val; break;
                    case "DATA":
                        solicitud.Data = string.IsNullOrWhiteSpace(val)
                            ? null
                            : JsonSerializer.Deserialize<JsonElement>(val);
                        break;
                }
            }

            // ── 1b. Token desde header Authorization (Bearer) ────────────
            if (string.IsNullOrWhiteSpace(solicitud.Token)
                && request.Headers != null)
            {
                var authHeader = request.Headers
                    .FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    .Value;

                if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    solicitud.Token = authHeader["Bearer ".Length..].Trim();
            }

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
            Body       = JsonSerializer.Serialize(body, _jsonOutputOptions),
            Headers    = new Dictionary<string, string>
            {
                ["Content-Type"]                 = "application/json",
                ["Access-Control-Allow-Origin"]   = "*",
                ["Access-Control-Allow-Headers"]  = "Content-Type,Authorization",
                ["Access-Control-Allow-Methods"]  = "POST,OPTIONS"
            }
        };
}
