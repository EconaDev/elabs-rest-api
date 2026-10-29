using ELabs.RestApi.Infrastructure.External.AWDW.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.External.AWDW;

public class AWDWDbContext(DbContextOptions<AWDWDbContext> options) : DbContext(options)
{
    public DbSet<AWDWCustomerView> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AWDWDbContext).Assembly);
    }

}
