using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence;

public interface ITibiaStalkerDbContext
{
    DatabaseFacade Database { get; }
    ChangeTracker ChangeTracker { get; }

    DbSet<World> Worlds { get; set; }
    DbSet<WorldScan> WorldScans { get; set; }
    DbSet<Character> Characters { get; set; }
    DbSet<CharacterCorrelation> CharacterCorrelations { get; set; }
    DbSet<TrackedCharacter> TrackedCharacters { get; set; }
    DbSet<OnlineCharacter> OnlineCharacters { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteRawSqlAsync(string rawSql, int? timeOut = null, CancellationToken cancellationToken = default);
}