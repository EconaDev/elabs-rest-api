namespace ELabs.RestApi.WebAPI.Middleware;

/// <summary>
/// Error body returned for validation failures (422) and unexpected server errors (500).
/// </summary>
/// <param name="Type">A URI reference that identifies the problem type.</param>
/// <param name="Title">A short, human-readable summary of the problem.</param>
/// <param name="Status">The HTTP status code.</param>
/// <param name="CorrelationId">The request correlation ID. Quote it when reporting a problem; it matches the X-Correlation-ID response header and the log and audit records.</param>
/// <param name="Detail">A human-readable explanation. The exception message in Development; a generic message otherwise.</param>
/// <param name="Errors">Validation errors keyed by field name. Present for 422 responses only.</param>
public sealed record ApiProblemDetails(
    string Type,
    string Title,
    int Status,
    string CorrelationId,
    string? Detail,
    IReadOnlyDictionary<string, string[]>? Errors);
