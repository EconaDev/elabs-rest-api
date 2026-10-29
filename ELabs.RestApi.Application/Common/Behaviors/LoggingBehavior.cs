using ELabs.RestApi.Application.Common.Context;
using MediatR;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Text.Json;

namespace ELabs.RestApi.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    IRequestContextAccessor contextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = contextAccessor.Current;
        var requestName = typeof(TRequest).Name;

        var sw = Stopwatch.StartNew();

        using var _ = BuildLogContext(context, requestName);

        try
        {
            var response = await next();
            sw.Stop();

            var elapsedUs = (long)(sw.ElapsedTicks * 1_000_000L / Stopwatch.Frequency);

            // CacheHit is only known after the pipeline ran; re-push it for the completion line.
            // Skip serializing the response on a cache hit.
            bool cacheHit = context.CacheHit;

            using (LogContext.PushProperty("ElapsedUs", elapsedUs))
            using (LogContext.PushProperty("CacheHit", cacheHit))
            using (LogContext.PushProperty("AfterJson", cacheHit ? null : SafeSerialize(response)))
            {
                Log.ForContext("ElapsedUs", elapsedUs)
                    .Information(
                        "Request completed | {RequestName} | {ElapsedUs}us",
                        requestName,
                        elapsedUs);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            var elapsedUs = (long)(sw.ElapsedTicks * 1_000_000L / Stopwatch.Frequency);

            Log.ForContext("ElapsedUs", elapsedUs)
                .Error(ex, "Request failed | {RequestName} | {ElapsedUs}us",
                    requestName, elapsedUs);

            throw;
        }
    }

    private static string? SafeSerialize(object? obj)
    {
        if (obj is null) return null;

        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch
        {
            return "[Serialization failed]";
        }
    }

    private static DisposableStack BuildLogContext(IRequestContext context, string requestName)
    {
        return new DisposableStack(
            LogContext.PushProperty("CorrelationId", context.CorrelationId),
            LogContext.PushProperty("RequestName", requestName),
            LogContext.PushProperty("UserEmail", context.UserEmail),
            LogContext.PushProperty("UserName", context.UserName),
            LogContext.PushProperty("FormName", context.FormName),
            LogContext.PushProperty("FormSection", context.FormSection),
            LogContext.PushProperty("ClientIP", context.IpAddress),
            LogContext.PushProperty("UserAgent", context.UserAgent),
            LogContext.PushProperty("CacheHit", context.CacheHit)
        );
    }

    /// <summary>
    /// Fixed-size disposable aggregator (no dynamic allocations, no Stack&lt;T&gt;)
    /// </summary>
    private sealed class DisposableStack(
        IDisposable d1,
        IDisposable d2,
        IDisposable d3,
        IDisposable d4,
        IDisposable d5,
        IDisposable d6,
        IDisposable d7,
        IDisposable d8,
        IDisposable d9) : IDisposable
    {
        private readonly IDisposable _d1 = d1;
        private readonly IDisposable _d2 = d2;
        private readonly IDisposable _d3 = d3;
        private readonly IDisposable _d4 = d4;
        private readonly IDisposable _d5 = d5;
        private readonly IDisposable _d6 = d6;
        private readonly IDisposable _d7 = d7;
        private readonly IDisposable _d8 = d8;
        private readonly IDisposable _d9 = d9;

        public void Dispose()
        {
            // Reverse order (stack semantics)
            _d9.Dispose();
            _d8.Dispose();
            _d7.Dispose();
            _d6.Dispose();
            _d5.Dispose();
            _d4.Dispose();
            _d3.Dispose();
            _d2.Dispose();
            _d1.Dispose();
        }
    }
}
