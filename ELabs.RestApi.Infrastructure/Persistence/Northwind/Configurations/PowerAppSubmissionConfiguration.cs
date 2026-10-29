using ELabs.RestApi.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind.Configurations;

public class PowerAppSubmissionConfiguration : IEntityTypeConfiguration<PowerAppSubmission>
{
    public void Configure(EntityTypeBuilder<PowerAppSubmission> builder)
    {
        builder.ToTable("PowerAppSubmissions");
        builder.HasKey(x => x.PowerAppId);
        builder.Property(x => x.PowerAppId).HasColumnName("PowerAppID").ValueGeneratedOnAdd();
        builder.Property(x => x.PowerAppName).HasMaxLength(100);
        builder.Property(x => x.FormName).HasMaxLength(50).HasDefaultValue("Unspecified");
        builder.Property(x => x.FormStatus).HasMaxLength(50).HasDefaultValue("Draft");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.SubmittedBy).HasMaxLength(100);
    }
}
