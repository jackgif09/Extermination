using Extermination.Models;
using Microsoft.EntityFrameworkCore;

namespace Extermination.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServiceRequest>().HasData(
            new ServiceRequest
            {
                Id = 1,
                CustomerName = "Alice Johnson",
                Phone = "555-123-4567",
                Email = "alice@example.com",
                Address = "123 Maple Street, Springfield, IL 62701",
                PestType = PestType.Ants,
                Description = "Ant trail coming in through kitchen window.",
                PreferredDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                Status = ServiceStatus.Completed,
                CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
            },
            new ServiceRequest
            {
                Id = 2,
                CustomerName = "Bob Martinez",
                Phone = "555-987-6543",
                Email = "bob@example.com",
                Address = "456 Oak Avenue, Shelbyville, IL 62565",
                PestType = PestType.Rodents,
                Description = "Hearing noises in the attic at night.",
                PreferredDate = new DateTime(2024, 7, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = ServiceStatus.Scheduled,
                CreatedAt = new DateTime(2024, 6, 28, 10, 30, 0, DateTimeKind.Utc)
            },
            new ServiceRequest
            {
                Id = 3,
                CustomerName = "Carol White",
                Phone = "555-246-8013",
                Email = "carol@example.com",
                Address = "789 Pine Road, Capital City, IL 62702",
                PestType = PestType.Termites,
                Description = "Noticed wood damage in basement beams.",
                PreferredDate = null,
                Status = ServiceStatus.New,
                CreatedAt = new DateTime(2024, 7, 1, 14, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
