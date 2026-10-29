using Serilog.Context;

namespace ELabs.RestApi.WebAPI.Middleware;

/// <summary>
/// Ensures each HTTP request has a unique correlation identifier.
/// Reads X-Correlation-ID from the request header (or falls back to TraceIdentifier),
/// stamps it onto TraceIdentifier, echoes it in the response header, and pushes it
/// into the Serilog LogContext so every downstream log entry carries it.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";

    /// <summary>
    /// Processes an HTTP request by assigning a correlation identifier and ensuring it is included in both the request and
    /// response headers.
    /// </summary>
    /// <remarks>If the incoming request does not include a valid correlation identifier header (1-64 characters of
    /// letters, digits, '-', '_' or '.'), the method uses the current trace identifier. The correlation identifier is made available for logging and is included in the response headers
    /// to support end-to-end request tracing.</remarks>
    /// <param name="context">The HTTP context for the current request. Must not be null.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Accept the caller's ID only if it is 1-64 safe characters; otherwise keep the generated one.
        string? incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        string correlationId = HeaderSanitizer.IsValidCorrelationId(incoming)
            ? incoming
            : context.TraceIdentifier;

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
