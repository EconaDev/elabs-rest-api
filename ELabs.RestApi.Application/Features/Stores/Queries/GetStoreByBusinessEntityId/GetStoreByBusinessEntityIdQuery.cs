using ELabs.RestApi.Application.Common.Interfaces.Caching;
using ELabs.RestApi.Application.Common.Interfaces.Audit;
using MediatR;

namespace ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;

public class GetStoreByBusinessEntityIdQuery
    : IRequest<GetStoreByBusinessEntityIdResponse?>,
      ICacheableRequest,
      IAuditableRequest
{
    public int BusinessEntityId { get; set; }

    // -------------------------------
    // CACHING
    // -------------------------------
    public string CacheKey => $"store:{BusinessEntityId}";
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration => null;

    // -------------------------------
    // AUDIT
    // -------------------------------
    public string ActionName => "GET_STORE_BY_BUSINESS_ENTITY_ID";
    public string RequestParams => $"BusinessEntityId:{BusinessEntityId}";
}
