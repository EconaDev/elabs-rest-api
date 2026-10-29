namespace ELabs.RestApi.Application.Common.DTOs.Audit;

public class AuditEntry
{
    public string? CorrelationId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = default!;
    public string? RequestName { get; set; }
    public string? RequestParams { get; set; }
    public string? FormName { get; set; }
    public string? FormSection { get; set; }
    public string? IpAddress { get; set; }
    public bool FromCache { get; set; }
    public string Status { get; set; } = default!;
}
