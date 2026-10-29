namespace ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;

public class GetStoreByBusinessEntityIdResponse
{
    public int ID { get; set; }
    public int BusinessEntityId { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? LegalName { get; set; }

}
