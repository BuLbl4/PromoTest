using Microsoft.EntityFrameworkCore;
using PromoTest.Api.Domain;

namespace PromoTest.Api.Persistence;

public class EfContext(DbContextOptions<EfContext> options) : DbContext(options)
{
    public DbSet<TaskItem> TaskItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EfContext).Assembly);
    }
}