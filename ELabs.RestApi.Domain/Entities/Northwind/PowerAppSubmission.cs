namespace ELabs.RestApi.Domain.Entities.Northwind;

public class PowerAppSubmission
{
    public int PowerAppId { get; set; }
    public string PowerAppName { get; set; } = default!;
    public string FormName { get; set; } = "Unspecified";
    public string FormStatus { get; set; } = "Draft";
    public DateTime CreatedDate { get; set; }
    public string SubmittedBy { get; set; } = default!;
}
