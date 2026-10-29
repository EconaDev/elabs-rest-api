namespace ELabs.RestApi.Application.Common.Context;

public record RequestContextSnapshot(
    string CorrelationId,
    string? UserEmail,
    string? UserName,
    string? IpAddress,
    string? UserAgent,
    string? FormName,
    string? FormSection,
    bool CacheHit
);
