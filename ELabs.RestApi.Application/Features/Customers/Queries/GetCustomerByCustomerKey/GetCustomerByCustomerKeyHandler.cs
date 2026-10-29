using ELabs.RestApi.Application.Features.Customers.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;

public class GetCustomerByCustomerKeyHandler(
    ICustomerRepository repository,
    ILogger<GetCustomerByCustomerKeyHandler> logger)
    : IRequestHandler<GetCustomerByCustomerKeyQuery, GetCustomerByCustomerKeyResponse?>
{
    private readonly ICustomerRepository _repository = repository;
    private readonly ILogger<GetCustomerByCustomerKeyHandler> _logger = logger;

    // -------- Precompiled log messages (zero-allocation) --------

    private static readonly Action<ILogger, int, Exception?> _fetchingCustomer =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(1101, nameof(GetCustomerByCustomerKeyHandler)),
            "Fetching customer {CustomerKey}");

    private static readonly Action<ILogger, int, bool, Exception?> _customerRetrieved =
        LoggerMessage.Define<int, bool>(
            LogLevel.Information,
            new EventId(1102, nameof(GetCustomerByCustomerKeyHandler)),
            "Customer retrieved {CustomerKey} Success={Success}");

    public async Task<GetCustomerByCustomerKeyResponse?> Handle(
        GetCustomerByCustomerKeyQuery request,
        CancellationToken cancellationToken)
    {
        _fetchingCustomer(_logger, request.CustomerKey, null);

        var result = await _repository.GetByCustomerKeyAsync(request.CustomerKey);

        _customerRetrieved(_logger, request.CustomerKey, result != null, null);

        return result;
    }
}
