using ELabs.RestApi.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind.Configurations;

public class PowerAppAttributeConfiguration : IEntityTypeConfiguration<PowerAppAttribute>
{
    public void Configure(EntityTypeBuilder<PowerAppAttribute> builder)
    {
        builder.ToTable("PowerAppAttributes");
        builder.HasKey(x => x.AttributeId);
        builder.Property(x => x.AttributeId).HasColumnName("AttributeID").ValueGeneratedOnAdd();
        builder.Property(x => x.AttributeName).HasMaxLength(100);
        builder.Property(x => x.FormName).HasMaxLength(50);
        builder.Property(x => x.DataType).HasMaxLength(50);
        builder.Property(x => x.AssociatedColumn).HasMaxLength(50);
        builder.HasIndex(x => new { x.FormName, x.AttributeName }).IsUnique().HasDatabaseName("UQ_PAA_FormScope");
    }
}
