namespace ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;

public class GetCustomerByCustomerKeyResponse
{
    public int CustomerKey { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Education { get; set; }
    public string? Occupation { get; set; }
    public string? Region { get; set; }
    public int Age { get; set; }
    public bool BikeBuyer { get; set; }
}
