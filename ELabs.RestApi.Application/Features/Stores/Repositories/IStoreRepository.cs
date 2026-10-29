using ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;

namespace ELabs.RestApi.Application.Features.Stores.Repositories;

public interface IStoreRepository
{
    Task<GetStoreByBusinessEntityIdResponse?> GetByBusinessEntityIdAsync(int businessEntityId);
}
