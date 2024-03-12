using Microsoft.EntityFrameworkCore;

namespace Consumer;

public class CustomerContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        Database.EnsureCreated();
    }
}

public record Customer (string Name, string Address)
{
    public Guid Id { get; set; } = Guid.NewGuid();
};