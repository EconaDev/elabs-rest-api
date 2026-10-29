using ELabs.RestApi.Infrastructure.External.AWDW.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.External.AWDW.Configuration;

public class AWDWCustomerViewConfiguration : IEntityTypeConfiguration<AWDWCustomerView>
{
    public void Configure(EntityTypeBuilder<AWDWCustomerView> builder)
    {
        builder.HasNoKey();
        builder.ToView("vTargetMail", "dbo");

        builder.Property(x => x.CustomerKey).HasColumnName("CustomerKey");
        builder.Property(x => x.FirstName).HasColumnName("FirstName");
        builder.Property(x => x.LastName).HasColumnName("LastName");
        builder.Property(x => x.Education).HasColumnName("EnglishEducation");
        builder.Property(x => x.Occupation).HasColumnName("EnglishOccupation");
        builder.Property(x => x.Region).HasColumnName("Region");
        builder.Property(x => x.Age).HasColumnName("Age");
        builder.Property(x => x.BikeBuyer).HasColumnName("BikeBuyer");
    }
}
