namespace ELabs.RestApi.Application.Common.Context;

public class RequestContext : IRequestContext
{
    public string CorrelationId { get; init; } = default!;
    public string? UserEmail { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? FormName { get; init; }
    public string? FormSection { get; init; }
    public bool CacheHit { get; set; }
    public RequestContextSnapshot Snapshot() =>
        new(
            CorrelationId,
            UserEmail,
            UserName,
            IpAddress,
            UserAgent,
            FormName,
            FormSection,
            CacheHit
        );
}
