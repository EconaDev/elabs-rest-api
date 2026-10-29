using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace ELabs.RestApi.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Attempted values are caller-controlled input that failed validation, so they are bounded
    // before they reach the log table (ValidationErrorsJson is nvarchar(max)).
    private const int MaxAttemptedValueLength = 200;
    private const int MaxValidationJsonLength = 4000;
    private const int TruncationMarkerReserve = 80;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var requestName = typeof(TRequest).Name;

        string validationJson = BuildValidationJson(failures);

        // One structured entry per failure; ExceptionMiddleware adds the method/path line.
        using (LogContext.PushProperty("ValidationErrorsJson", validationJson))
        {
            Log.Warning(
                "Validation failed | {RequestName} | {ErrorCount} error(s)",
                requestName,
                failures.Count);
        }

        throw new ValidationException(failures);
    }

    /// <summary>
    /// Builds a JSON array of failures. Each attempted value is truncated, and when the array would
    /// exceed the size cap whole entries are dropped (with a marker entry), so the result is always
    /// valid JSON.
    /// </summary>
    private static string BuildValidationJson(IReadOnlyList<ValidationFailure> failures)
    {
        List<string> parts = new(failures.Count);
        int length = 2; // '[' and ']'
        int omitted = 0;

        for (int i = 0; i < failures.Count; i++)
        {
            ValidationFailure failure = failures[i];

            string part = JsonSerializer.Serialize(new
            {
                Field = failure.PropertyName,
                Error = failure.ErrorMessage,
                AttemptedValue = TruncateValue(failure.AttemptedValue)
            });

            // +1 for the comma separator
            if (length + part.Length + 1 > MaxValidationJsonLength - TruncationMarkerReserve)
            {
                omitted = failures.Count - i;
                break;
            }

            parts.Add(part);
            length += part.Length + 1;
        }

        if (omitted > 0)
        {
            parts.Add(JsonSerializer.Serialize(new
            {
                Field = "(truncated)",
                Error = $"{omitted} more error(s) omitted"
            }));
        }

        return $"[{string.Join(',', parts)}]";
    }

    private static string? TruncateValue(object? value)
    {
        string? text = value?.ToString();

        if (text is null || text.Length <= MaxAttemptedValueLength)
            return text;

        int cut = MaxAttemptedValueLength;

        // Do not cut a surrogate pair in half.
        if (char.IsHighSurrogate(text[cut - 1]))
            cut--;

        return string.Concat(text.AsSpan(0, cut), "…");
    }
}
