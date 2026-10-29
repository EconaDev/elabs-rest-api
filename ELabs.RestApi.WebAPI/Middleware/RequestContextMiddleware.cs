using ELabs.RestApi.Application.Common.Context;

namespace ELabs.RestApi.WebAPI.Middleware;

/// <summary>
/// Builds the per-request <see cref="IRequestContext"/> and stores it in
/// <c>HttpContext.Items</c> so it is available to the entire MediatR pipeline.
///
/// IMPORTANT: must be registered AFTER <see cref="CorrelationIdMiddleware"/> so that
/// <c>HttpContext.TraceIdentifier</c> has already been set to the correct correlation ID.
/// </summary>
public class RequestContextMiddleware(RequestDelegate next)
{
    private const string Key = "REQUEST_CONTEXT";

    /// <summary>
    /// Processes an HTTP request by extracting request context information and invoking the next middleware in the
    /// pipeline.
    /// </summary>
    /// <remarks>This method captures key request metadata, such as correlation ID, user information, and client IP
    /// address, and stores it in the request context for downstream components. The client IP is the connection's
    /// remote address; X-Forwarded-For is never read directly. The method must be called as part of the ASP.NET Core
    /// middleware pipeline.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Provides access to request and response data, as well as features and user
    /// information. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation of the middleware invocation.</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Never read X-Forwarded-For directly: any client can set it. RemoteIpAddress is the
        // socket address, or the validated de-proxied address if UseForwardedHeaders() is
        // configured with KnownProxies (none today; no proxy sits in front of IIS).
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // TraceIdentifier has already been set by CorrelationIdMiddleware.
        var context = new RequestContext
        {
            CorrelationId = httpContext.TraceIdentifier,

            // Client-supplied: sanitized and truncated to the audit/log column lengths.
            UserEmail = HeaderSanitizer.Clean(httpContext.Request.Headers["x-user-email"].FirstOrDefault(), HeaderSanitizer.MaxIdentity),
            UserName = HeaderSanitizer.Clean(httpContext.Request.Headers["x-user-name"].FirstOrDefault(), HeaderSanitizer.MaxIdentity),

            IpAddress = ip,
            UserAgent = HeaderSanitizer.Clean(httpContext.Request.Headers.UserAgent.ToString(), HeaderSanitizer.MaxUserAgent),

            FormName = HeaderSanitizer.Clean(httpContext.Request.Headers["x-form-name"].FirstOrDefault(), HeaderSanitizer.MaxIdentity),
            FormSection = HeaderSanitizer.Clean(httpContext.Request.Headers["x-form-section"].FirstOrDefault(), HeaderSanitizer.MaxIdentity),

            CacheHit = false
        };

        httpContext.Items[Key] = context;

        await next(httpContext);
    }
}
