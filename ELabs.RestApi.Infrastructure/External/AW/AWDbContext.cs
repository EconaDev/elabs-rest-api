using ELabs.RestApi.Infrastructure.External.AW.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.External.AW;

public class AWDbContext(DbContextOptions<AWDbContext> options) : DbContext(options)
{
    public DbSet<AWStoreView> Stores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AWDbContext).Assembly);
    }

}
