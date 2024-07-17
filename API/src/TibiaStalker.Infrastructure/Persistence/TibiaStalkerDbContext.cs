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

        #region Worlds

        modelBuilder.Entity<World>(e =>
        {
            e.Property(w => w.Name)
                .HasMaxLength(20)
                .IsRequired();

            e.Property(w => w.Url)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(w => w.IsAvailable)
                .IsRequired();

            e.HasMany(w => w.WorldScans)
                .WithOne(ws => ws.World)
                .HasForeignKey(ws => ws.WorldId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasMany(w => w.Characters)
                .WithOne(c => c.World)
                .HasForeignKey(c => c.WorldId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        #endregion

        #region WorldScans

        modelBuilder.Entity<WorldScan>(e =>
        {
            e.HasKey(ws => ws.WorldScanId);
            e.HasIndex(ws => ws.WorldId);
            e.HasIndex(ws => ws.ScanCreateDateTime);
            e.HasIndex(ws => ws.IsDeleted);
            e.HasIndex(ws => new { ws.WorldId, ws.IsDeleted })
                .HasDatabaseName("ix_world_scan_id_world_id_is_deleted");
            e.HasIndex(ws => new { ws.WorldId, ws.IsDeleted, ws.ScanCreateDateTime })
                .HasDatabaseName("ix_world_scan_world_id_is_deleted_scan_date_time");

            e.Property(ws => ws.WorldScanId)
                .IsRequired();

            e.Property(ws => ws.CharactersOnline)
                .IsRequired();

            e.Property(ws => ws.WorldId)
                .IsRequired();

            e.Property(ws => ws.ScanCreateDateTime)
                .IsRequired();

            e.Property(ws => ws.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
        });

        #endregion

        #region Characters

        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.CharacterId);
            e.HasIndex(c => c.Name);
            e.HasIndex(c => c.WorldId);
            e.HasIndex(c => c.VerifiedDate);
            e.HasIndex(c => c.TradedDate);

            e.Property(c => c.CharacterId)
                .IsRequired();

            e.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(c => c.VerifiedDate)
                .IsRequired()
                .HasDefaultValue(new DateOnly(2001, 01, 01));

            e.Property(c => c.TradedDate)
                .IsRequired()
                .HasDefaultValue(new DateOnly(2001, 01, 01));

            e.HasMany(c => c.LogoutCharacterCorrelations)
                .WithOne(cc => cc.LogoutCharacter)
                .HasForeignKey(cc => cc.LogoutCharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(c => c.LoginCharacterCorrelations)
                .WithOne(cc => cc.LoginCharacter)
                .HasForeignKey(cc => cc.LoginCharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.DeleteApproachNumber)
                .IsRequired()
                .HasDefaultValue(0);
        });

        #endregion

        #region CharacterCorrelations

        modelBuilder.Entity<CharacterCorrelation>(e =>
        {
            e.HasKey(cc => cc.CorrelationId);
            e.HasIndex(cc => cc.LogoutCharacterId);
            e.HasIndex(cc => cc.LoginCharacterId);
            e.HasIndex(cc => cc.NumberOfMatches);

            e.Property(cc => cc.CorrelationId)
                .IsRequired();

            e.Property(cc => cc.LogoutCharacterId)
                .IsRequired();

            e.Property(cc => cc.LoginCharacterId)
                .IsRequired();

            e.Property(cc => cc.NumberOfMatches)
                .IsRequired();

            e.Property(cc => cc.CreateDate)
                .IsRequired();

            e.Property(cc => cc.LastMatchDate)
                .IsRequired();
        });

        #endregion

        #region TrackedCharacters

        modelBuilder.Entity<TrackedCharacter>(e =>
        {
            e.HasIndex(tc => tc.Name);
            e.HasIndex(tc => tc.WorldName);
            e.HasKey(tc => new { tc.Name, tc.ConnectionId });

            e.Property(tc => tc.Name)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(tc => tc.WorldName)
                .HasMaxLength(20)
                .IsRequired();

            e.Property(tc => tc.ConnectionId)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(tc => tc.StartTrackDateTime)
                .IsRequired();
        });

        #endregion

        #region OnlineCharacters

        modelBuilder.Entity<OnlineCharacter>(e =>
        {
            e.HasKey(oc => new {oc.Name, oc.OnlineDateTime});
            e.HasIndex(oc => oc.WorldName);

            e.Property(oc => oc.Name)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(oc => oc.WorldName)
                .HasMaxLength(20)
                .IsRequired();

            e.Property(oc => oc.OnlineDateTime)
                .IsRequired();
        });

        #endregion
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