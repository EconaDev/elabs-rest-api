namespace ELabs.RestApi.Application.Common.Context;

public interface IRequestContext
{
    string CorrelationId { get; }
    string? UserEmail { get; }
    string? UserName { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? FormName { get; }
    string? FormSection { get; }
    bool CacheHit { get; set; }
    RequestContextSnapshot Snapshot();
}
