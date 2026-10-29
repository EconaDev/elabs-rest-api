namespace ELabs.RestApi.Domain.Entities.Northwind;

public class PowerAppAttributeValue
{
    public int PowerAppValueId { get; set; }
    public int PowerAppId { get; set; }
    public int AttributeId { get; set; }
    public int InstanceNumber { get; set; } = 1;
    public string? ValueText { get; set; }
    public decimal? ValueNumber { get; set; }
    public DateOnly? ValueDate { get; set; }
    public bool? ValueBoolean { get; set; }
}
