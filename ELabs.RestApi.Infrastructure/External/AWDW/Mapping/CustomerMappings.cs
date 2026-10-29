using ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;
using ELabs.RestApi.Infrastructure.External.AWDW.Entities;

namespace ELabs.RestApi.Infrastructure.External.AWDW.Mapping;

public static class CustomerMappings
{
    public static GetCustomerByCustomerKeyResponse ToResponse(this AWDWCustomerView source)
    {
        return new GetCustomerByCustomerKeyResponse
        {
            CustomerKey = source.CustomerKey,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Education = source.Education,
            Occupation = source.Occupation,
            Region = source.Region,
            Age = source.Age,
            BikeBuyer = source.BikeBuyer == 1
        };
    }
}
