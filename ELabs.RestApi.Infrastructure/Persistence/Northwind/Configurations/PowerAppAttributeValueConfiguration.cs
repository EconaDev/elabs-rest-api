using ELabs.RestApi.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind.Configurations;

public class PowerAppAttributeValueConfiguration : IEntityTypeConfiguration<PowerAppAttributeValue>
{
    public void Configure(EntityTypeBuilder<PowerAppAttributeValue> builder)
    {
        builder.ToTable("PowerAppAttributeValues");
        builder.HasKey(x => x.PowerAppValueId);
        builder.Property(x => x.PowerAppValueId).HasColumnName("PowerAppValueID").ValueGeneratedOnAdd();
        builder.Property(x => x.PowerAppId).HasColumnName("PowerAppID");
        builder.Property(x => x.AttributeId).HasColumnName("AttributeID");
        builder.Property(x => x.InstanceNumber).HasDefaultValue(1);
        builder.Property(x => x.ValueNumber).HasColumnType("numeric(18,5)");
        builder.Property(x => x.ValueDate).HasColumnType("date");
        builder.HasIndex(x => new { x.PowerAppId, x.AttributeId, x.InstanceNumber }).IsUnique().HasDatabaseName("UQ_PAAV");
        builder.HasOne<PowerAppAttribute>().WithMany().HasForeignKey(x => x.AttributeId).HasConstraintName("FK_PAAV_PowerAppAttributes");
        builder.HasOne<PowerAppSubmission>().WithMany().HasForeignKey(x => x.PowerAppId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_PAAV_Submissions");
    }
}
