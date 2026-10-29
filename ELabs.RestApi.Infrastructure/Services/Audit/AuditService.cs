using ELabs.RestApi.Application.Common.DTOs.Audit;
using ELabs.RestApi.Application.Common.Interfaces.Audit;
using ELabs.RestApi.Infrastructure.Persistence.Northwind;
using ELabs.RestApi.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.Services.Audit;

/// <summary>
/// Persists audit entries to the database.
/// No direct dependency on IHttpContextAccessor; all context arrives via AuditEntry.
/// </summary>
public class AuditService(NorthwindDbContext context) : IAuditService
{
    public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        var audit = new AuditLog
        {
            CorrelationId = entry.CorrelationId,
            UserEmail = entry.UserEmail,
            UserName = entry.UserName,
            Action = entry.Action,
            RequestName = entry.RequestName,
            RequestParams = entry.RequestParams,
            FormName = entry.FormName,
            FormSection = entry.FormSection,
            IpAddress = entry.IpAddress,
            FromCache = entry.FromCache,
            Status = entry.Status ?? "SUCCESS",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.AuditLogs.Add(audit);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Do not leave the failed entry tracked: a later SaveChanges in this scope would try to
            // insert it again.
            context.Entry(audit).State = EntityState.Detached;
            throw;
        }
    }
}
