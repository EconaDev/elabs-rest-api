namespace ELabs.RestApi.Infrastructure.External.AW.Entities;

// AdventureWorks stand-in: backed by Sales.vStoreWithAddresses (see AWStoreViewConfiguration).
public class AWStoreView
{
    public int StoreID { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
}
