using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ELabs.RestApi.WebAPI.Middleware;

/// <summary>
/// Exception handling middleware that catches unhandled exceptions, logs them with Serilog, and returns standardized ProblemDetails responses to clients. It maps ValidationException to 422 and every other exception to 500, ignores client disconnects, and includes a correlation ID for traceability. In development environments, it also includes exception details in the response for easier debugging.
/// </summary>
/// <remarks>
/// ExceptionMiddleware constructor with dependencies injected via DI. The RequestDelegate represents the next middleware in the pipeline, ILogger is used for logging exceptions, and IHostEnvironment allows the middleware to determine if it's running in a development environment to include detailed error information in responses.
/// </remarks>
/// <param name="next"></param>
/// <param name="logger"></param>
/// <param name="env"></param>
public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;

    /// <summary>
    /// InvokeAsync is the main method of the middleware that gets called for each HTTP request. It wraps the call to the next middleware in a try-catch block to catch any unhandled exceptions. When an exception is caught, it logs the exception details along with the HTTP method, path, and correlation ID. If the response has not already started, it then writes a ProblemDetails response back to the client with an appropriate status code and error information based on the type of exception.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; there is nobody to send a response to.
            LogClientAborted(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;

            LogException(context, ex, correlationId);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Response already started. Skipping error response. CorrelationId: {CorrelationId}",
                    correlationId);

                throw;
            }

            try
            {
                await WriteProblemDetailsAsync(context, ex, correlationId);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // The client disconnected while the error response was being written.
                LogClientAborted(context);
            }
        }
    }

    private void LogClientAborted(HttpContext context)
    {
        _logger.LogDebug(
            "Request aborted by client. Method: {Method} Path: {Path} CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);
    }

    private void LogException(HttpContext context, Exception ex, string correlationId)
    {
        if (ex is ValidationException vex)
        {
            _logger.LogWarning(
                "Validation failed. Method: {Method} Path: {Path} Errors: {@ValidationErrors} CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                correlationId);
            return;
        }

        _logger.LogError(
            ex,
            "Unhandled exception occurred. Method: {Method} Path: {Path} CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);
    }

    private Task WriteProblemDetailsAsync(
        HttpContext context,
        Exception ex,
        string correlationId)
    {
        var (statusCode, title) = MapException(ex);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        ApiProblemDetails problem = BuildProblemDetails(ex, statusCode, title, correlationId);

        var json = JsonSerializer.Serialize(problem, JsonOptions());

        return context.Response.WriteAsync(json, context.RequestAborted);
    }

    // Only ValidationException is a deliberate client error. Everything else is a server fault: broad
    // .NET exceptions (ArgumentException, KeyNotFoundException, UnauthorizedAccessException) are
    // thrown by libraries and the file system on server problems and must not be reported as 4xx.
    // If a handler needs a deliberate 400/404/401/403, add an explicit exception type and map it here.
    private static (HttpStatusCode Status, string Title) MapException(Exception ex)
    {
        return ex switch
        {
            ValidationException => (HttpStatusCode.UnprocessableEntity, "Validation failed"),
            _ => (HttpStatusCode.InternalServerError, "Server error")
        };
    }

    private ApiProblemDetails BuildProblemDetails(
        Exception ex,
        HttpStatusCode status,
        string title,
        string correlationId)
    {
        string type = $"https://httpstatuses.com/{(int)status}";

        if (ex is ValidationException validationEx)
        {
            IReadOnlyDictionary<string, string[]> validationErrors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            return new ApiProblemDetails(type, title, (int)status, correlationId, Detail: null, validationErrors);
        }

        bool isDev = _env.IsDevelopment();

        string? detail = isDev ? ex.Message : "An unexpected error occurred.";

        return new ApiProblemDetails(type, title, (int)status, correlationId, detail, Errors: null);

    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}
