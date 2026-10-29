using ELabs.RestApi.Application.Common.Context;
using ELabs.RestApi.Application.Common.Interfaces.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ELabs.RestApi.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that serves <see cref="ICacheableRequest"/> responses from memory.
///
/// Registration order note: this behavior sits INSIDE Logging, Audit and Validation (see
/// <c>DependencyInjection</c>), so hits and misses are logged, audited and validated by the same
/// code. It only sets <see cref="IRequestContext.CacheHit"/>; Audit reads it after <c>next()</c>.
///
/// Null responses are never cached: a not-found must not hide a record that appears later.
/// There is no per-key locking, so concurrent misses can each run the handler.
/// </summary>
public class CachingBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    IRequestContextAccessor contextAccessor,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string RequestName = typeof(TRequest).Name;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableRequest cacheable)
            return await next();

        IRequestContext context = contextAccessor.Current;
        string cacheKey = cacheable.CacheKey;

        // -------- CACHE HIT --------
        if (cache.TryGetValue(cacheKey, out TResponse? cached) && cached is not null)
        {
            context.CacheHit = true;
            LogDiagnostic("CACHE HIT ", cacheKey, cacheHit: true);
            return cached;
        }

        // -------- CACHE MISS --------
        context.CacheHit = false;
        LogDiagnostic("CACHE MISS", cacheKey, cacheHit: false);

        TResponse response = await next();

        if (response is not null)
        {
            cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    cacheable.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5),
                SlidingExpiration = cacheable.SlidingExpiration
            });
        }

        return response;
    }

    private void LogDiagnostic(string label, string cacheKey, bool cacheHit)
    {
        if (!logger.IsEnabled(LogLevel.Information))
            return;

        // The diagnostic line carries its own CacheHit value; the property pushed by
        // LoggingBehavior at request start is still the initial false.
        using (LogContext.PushProperty("CacheHit", cacheHit))
        {
            logger.LogInformation(
                "{Label} | Key: {CacheKey} | Request: {Request}",
                label, cacheKey, RequestName);
        }
    }
}
