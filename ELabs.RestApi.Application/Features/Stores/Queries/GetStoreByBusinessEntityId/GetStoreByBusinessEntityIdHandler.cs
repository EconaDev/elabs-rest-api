using ELabs.RestApi.Application.Features.Stores.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;

public class GetStoreByBusinessEntityIdHandler(
    IStoreRepository repository,
    ILogger<GetStoreByBusinessEntityIdHandler> logger)
    : IRequestHandler<GetStoreByBusinessEntityIdQuery, GetStoreByBusinessEntityIdResponse?>
{
    private readonly IStoreRepository _repository = repository;
    private readonly ILogger<GetStoreByBusinessEntityIdHandler> _logger = logger;

    // -------- Precompiled log messages (zero-allocation) --------

    private static readonly Action<ILogger, int, Exception?> _fetchingStore =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(1001, nameof(GetStoreByBusinessEntityIdHandler)),
            "Fetching store {BusinessEntityId}");

    private static readonly Action<ILogger, int, bool, Exception?> _storeRetrieved =
        LoggerMessage.Define<int, bool>(
            LogLevel.Information,
            new EventId(1002, nameof(GetStoreByBusinessEntityIdHandler)),
            "Store retrieved {BusinessEntityId} Success={Success}");

    public async Task<GetStoreByBusinessEntityIdResponse?> Handle(
        GetStoreByBusinessEntityIdQuery request,
        CancellationToken cancellationToken)
    {
        // DEBUG: Fetch start
        _fetchingStore(_logger, request.BusinessEntityId, null);

        var result = await _repository.GetByBusinessEntityIdAsync(request.BusinessEntityId);

        // INFO: Fetch result
        _storeRetrieved(_logger, request.BusinessEntityId, result != null, null);

        return result;
    }
}
