using ELabs.RestApi.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind.Configurations;

public class SubmissionDataConfiguration : IEntityTypeConfiguration<SubmissionData>
{
    public void Configure(EntityTypeBuilder<SubmissionData> builder)
    {
        builder.ToTable("Submission_Data");
        builder.HasKey(x => x.FileId);
        builder.Property(x => x.FileId).HasColumnName("fileid").ValueGeneratedOnAdd();
        builder.Property(x => x.Project).HasColumnName("project").HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.FileName).HasColumnName("filename").HasMaxLength(1000).IsUnicode(false);
        builder.Property(x => x.Field).HasColumnName("field").HasMaxLength(1000).IsUnicode(false);
        builder.Property(x => x.Value).HasColumnName("value").IsUnicode(false);
        builder.Property(x => x.Created).HasColumnName("created");
        builder.Property(x => x.LoadId).HasColumnName("loadid");
    }
}
