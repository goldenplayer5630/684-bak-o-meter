using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Data.Persistence;

/// <summary>
/// EF Core database context. Targets PostgreSQL via the Npgsql provider.
/// All entity configurations are applied automatically from the assembly.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<ChugAttempt> ChugAttempts => Set<ChugAttempt>();
    public DbSet<OneVsOneMatch> OneVsOneMatches => Set<OneVsOneMatch>();
    public DbSet<NfcTag> NfcTags => Set<NfcTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up PlayerConfig, ChugAttemptConfig, OneVsOneMatchConfig automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
