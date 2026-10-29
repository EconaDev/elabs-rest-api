using ELabs.RestApi.Application.Common.DTOs.Audit;

namespace ELabs.RestApi.Application.Common.Interfaces.Audit;

public interface IAuditService
{
    Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
