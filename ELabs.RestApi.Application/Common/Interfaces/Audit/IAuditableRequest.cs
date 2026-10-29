namespace ELabs.RestApi.Application.Common.Interfaces.Audit;

/// <summary>
/// Marker interface for requests that should be automatically audited.
/// </summary>
/// <remarks>
/// Convention: <c>AuditBehavior</c> records a null response as <c>NOT_FOUND</c>. A request whose handler
/// returns null for any other reason must not implement this interface until the audit takes an explicit
/// outcome.
/// </remarks>
public interface IAuditableRequest
{
    string ActionName { get; }
    string? RequestParams { get; }
}
