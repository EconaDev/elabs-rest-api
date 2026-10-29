namespace ELabs.RestApi.Domain.Entities.Northwind;

public class PowerAppAttribute
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = default!;
    public string FormName { get; set; } = default!;
    public string DataType { get; set; } = default!;
    public string AssociatedColumn { get; set; } = default!;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
}
