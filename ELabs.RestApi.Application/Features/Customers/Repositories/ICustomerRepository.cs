using ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;

namespace ELabs.RestApi.Application.Features.Customers.Repositories;

public interface ICustomerRepository
{
    Task<GetCustomerByCustomerKeyResponse?> GetByCustomerKeyAsync(int customerKey);
}
