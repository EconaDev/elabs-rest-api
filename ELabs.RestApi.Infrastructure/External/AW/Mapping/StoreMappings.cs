using ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;
using ELabs.RestApi.Infrastructure.External.AW.Entities;

namespace ELabs.RestApi.Infrastructure.External.AW.Mapping;

public static class StoreMappings
{
    public static GetStoreByBusinessEntityIdResponse ToResponse(this AWStoreView source)
    {
        return new GetStoreByBusinessEntityIdResponse
        {
            ID = source.StoreID,
            BusinessEntityId = source.StoreID,
            Name = source.Name,
            City = source.City,
            State = source.State,
            Zip = source.Zip,
            LegalName = source.Name
        };
    }
}
