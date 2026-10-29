using ELabs.RestApi.Domain.Entities.Northwind;
using ELabs.RestApi.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;

namespace ELabs.RestApi.Infrastructure.Persistence.Northwind;

public class NorthwindDbContext(DbContextOptions<NorthwindDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs { get; set; } = default!;
    public DbSet<FormSubmission> FormSubmissions { get; set; } = default!;
    public DbSet<SubmissionData> SubmissionData { get; set; } = default!;
    public DbSet<PowerAppAttribute> PowerAppAttributes { get; set; } = default!;
    public DbSet<PowerAppSubmission> PowerAppSubmissions { get; set; } = default!;
    public DbSet<PowerAppAttributeValue> PowerAppAttributeValues { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NorthwindDbContext).Assembly);
    }

}
