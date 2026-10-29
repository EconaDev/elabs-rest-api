namespace ELabs.RestApi.Infrastructure.External.AWDW.Entities;

// AdventureWorksDW stand-in: backed by dbo.vTargetMail (see AWDWCustomerViewConfiguration).
public class AWDWCustomerView
{
    public int CustomerKey { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Education { get; set; }
    public string? Occupation { get; set; }
    public string? Region { get; set; }
    public int Age { get; set; }
    public int BikeBuyer { get; set; }
}
