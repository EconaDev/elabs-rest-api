using ELabs.RestApi.Domain.Entities.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("Form_Submissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        builder.Property(x => x.FileName).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.FormName).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.DbProcStatus).HasColumnName("DBProcStatus");
    }
}
