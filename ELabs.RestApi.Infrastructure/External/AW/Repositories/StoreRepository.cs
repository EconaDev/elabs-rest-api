using ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;
using ELabs.RestApi.Application.Features.Stores.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.External.AW.Repositories;

public class StoreRepository(AWDbContext context) : IStoreRepository
{
    private readonly AWDbContext _context = context;

    public async Task<GetStoreByBusinessEntityIdResponse?> GetByBusinessEntityIdAsync(int id)
    {
        var entity = await _context.Stores
            .FirstOrDefaultAsync(x => x.StoreID == id);

        if (entity == null)
            return null;

        return new GetStoreByBusinessEntityIdResponse
        {
            ID = entity.StoreID,
            BusinessEntityId = entity.StoreID,
            Name = entity.Name,
            City = entity.City,
            State = entity.State,
            Zip = entity.Zip,
            LegalName = entity.Name
        };
    }
}
