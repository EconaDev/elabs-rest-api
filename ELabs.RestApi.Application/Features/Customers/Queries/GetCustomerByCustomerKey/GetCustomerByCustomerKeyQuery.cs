using ELabs.RestApi.Application.Common.Interfaces.Caching;
using ELabs.RestApi.Application.Common.Interfaces.Audit;
using MediatR;

namespace ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;

public class GetCustomerByCustomerKeyQuery
    : IRequest<GetCustomerByCustomerKeyResponse?>,
      ICacheableRequest,
      IAuditableRequest
{
    public int CustomerKey { get; set; }

    // -------------------------------
    // CACHING
    // -------------------------------
    public string CacheKey => $"customer:{CustomerKey}";
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration => null;

    // -------------------------------
    // AUDIT
    // -------------------------------
    public string ActionName => "GET_CUSTOMER_BY_CUSTOMER_KEY";
    public string RequestParams => $"CustomerKey:{CustomerKey}";
}
