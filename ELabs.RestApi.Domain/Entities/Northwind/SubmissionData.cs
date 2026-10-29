namespace ELabs.RestApi.Domain.Entities.Northwind;

public class SubmissionData
{
    public int FileId { get; set; }
    public string? Project { get; set; }
    public string? FileName { get; set; }
    public string? Field { get; set; }
    public string? Value { get; set; }
    public DateTime? Created { get; set; }
    public int? LoadId { get; set; }
}
