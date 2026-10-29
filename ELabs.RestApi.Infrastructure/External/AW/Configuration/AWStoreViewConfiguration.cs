using ELabs.RestApi.Infrastructure.External.AW.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELabs.RestApi.Infrastructure.External.AW.Configuration;

public class AWStoreViewConfiguration : IEntityTypeConfiguration<AWStoreView>
{
    public void Configure(EntityTypeBuilder<AWStoreView> builder)
    {
        builder.HasNoKey();
        builder.ToView("vStoreWithAddresses", "Sales");

        builder.Property(x => x.StoreID).HasColumnName("BusinessEntityID");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.City).HasColumnName("City");
        builder.Property(x => x.State).HasColumnName("StateProvinceName");
        builder.Property(x => x.Zip).HasColumnName("PostalCode");
    }
}
