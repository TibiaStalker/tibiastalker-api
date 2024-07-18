using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence;

public class TibiaStalkerDbContext : DbContext, ITibiaStalkerDbContext
{
    public TibiaStalkerDbContext(DbContextOptions<TibiaStalkerDbContext> options) : base(options)
    {
    }

    public DbSet<World> Worlds { get; set; }
    public DbSet<WorldScan> WorldScans { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<CharacterCorrelation> CharacterCorrelations { get; set; }
    public DbSet<TrackedCharacter> TrackedCharacters { get; set; }
    public DbSet<OnlineCharacter> OnlineCharacters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <param name="rawSql">Sql command to execute</param>
    /// <param name="timeOut">Optional value in seconds</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    public async Task ExecuteRawSqlAsync(string rawSql, int? timeOut = null, CancellationToken cancellationToken = default)
    {
        if (timeOut is not null)
        {
            Database.SetCommandTimeout(TimeSpan.FromSeconds((double)timeOut));
        }

        await Database.ExecuteSqlRawAsync(rawSql, cancellationToken);
    }
}