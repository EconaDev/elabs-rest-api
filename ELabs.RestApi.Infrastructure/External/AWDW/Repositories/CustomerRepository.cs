using ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;
using ELabs.RestApi.Application.Features.Customers.Repositories;
using ELabs.RestApi.Infrastructure.External.AWDW.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.External.AWDW.Repositories;

public class CustomerRepository(AWDWDbContext context) : ICustomerRepository
{
    private readonly AWDWDbContext _context = context;

    public async Task<GetCustomerByCustomerKeyResponse?> GetByCustomerKeyAsync(int customerKey)
    {
        var entity = await _context.Customers
            .FirstOrDefaultAsync(x => x.CustomerKey == customerKey);

        return entity?.ToResponse();
    }
}
