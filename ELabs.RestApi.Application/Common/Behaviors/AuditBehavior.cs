using ELabs.RestApi.Application.Common.Context;
using ELabs.RestApi.Application.Common.DTOs.Audit;
using ELabs.RestApi.Application.Common.Interfaces.Audit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELabs.RestApi.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that writes an audit record for every request that
/// implements <see cref="IAuditableRequest"/>.
///
/// Audit is operational traceability, not a compliance requirement, so it fails OPEN: a failed
/// audit write is logged at Error and never changes the outcome of the request. Only a failure
/// of the business operation (<c>next()</c>) writes a FAILURE row, and the original exception is
/// always rethrown. A failed SUCCESS/NOT_FOUND write never produces a FAILURE row.
/// </summary>
public class AuditBehavior<TRequest, TResponse>(
    IAuditService auditService,
    IRequestContextAccessor contextAccessor,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuditableRequest auditable)
            return await next();

        IRequestContext ctx = contextAccessor.Current;
        TResponse response;

        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            // A client abort is not a failure of the operation, so it is not audited.
            bool clientAborted = ex is OperationCanceledException && cancellationToken.IsCancellationRequested;

            if (!clientAborted)
                await TryWriteAsync(auditable, ctx.Snapshot(), "FAILURE", fromCache: false, cancellationToken);

            throw;
        }

        // Snapshot AFTER next() so CacheHit is already set by CachingBehavior.
        RequestContextSnapshot snapshot = ctx.Snapshot();

        // A null response is a not-found (the controllers return 404). The audit cannot see the
        // HTTP status, so this holds only while null means not-found for every IAuditableRequest.
        string status = response is null ? "NOT_FOUND" : "SUCCESS";

        await TryWriteAsync(auditable, snapshot, status, snapshot.CacheHit, cancellationToken);

        return response;
    }

    private async Task TryWriteAsync(
        IAuditableRequest auditable,
        RequestContextSnapshot snapshot,
        string status,
        bool fromCache,
        CancellationToken cancellationToken)
    {
        try
        {
            await auditService.WriteAsync(new AuditEntry
            {
                CorrelationId = snapshot.CorrelationId,
                UserEmail = snapshot.UserEmail,
                UserName = snapshot.UserName,

                Action = auditable.ActionName,
                RequestName = typeof(TRequest).Name,
                RequestParams = auditable.RequestParams,

                FormName = snapshot.FormName,
                FormSection = snapshot.FormSection,
                IpAddress = snapshot.IpAddress,

                FromCache = fromCache,
                Status = status
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The client went away while the audit row was being written; nothing to report.
        }
        catch (Exception ex)
        {
            // Fail open. Reaches the SQL sink and the fallback file sink (Error and above).
            logger.LogError(
                ex,
                "Audit write failed (fail open). Action: {Action} Status: {Status} CorrelationId: {CorrelationId}",
                auditable.ActionName,
                status,
                snapshot.CorrelationId);
        }
    }
}
