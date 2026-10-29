namespace ELabs.RestApi.Domain.Entities.Northwind;

public class FormSubmission
{
    public int Id { get; set; }
    public string? FileName { get; set; }
    public string? FormName { get; set; }
    public int? ProcessStatus { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public short? DbProcStatus { get; set; }
}
