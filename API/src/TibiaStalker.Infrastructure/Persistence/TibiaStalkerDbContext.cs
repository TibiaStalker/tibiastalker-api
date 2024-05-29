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
    public DbSet<CharacterAction> CharacterActions { get; set; }
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

            e.HasMany(w => w.CharacterLogoutOrLogins)
                .WithOne(ca => ca.World)
                .HasForeignKey(ca => ca.WorldId)
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
            e.HasIndex(c => c.FoundInScan1);
            e.HasIndex(c => c.FoundInScan2);
            e.HasIndex(c => c.VerifiedDate);
            e.HasIndex(c => c.TradedDate);
            e.HasIndex(c => new { c.FoundInScan1, c.FoundInScan2 })
                .HasDatabaseName("ix_characters_scan1_scan2");
            e.HasIndex(c => new { c.FoundInScan2, c.FoundInScan1 })
                .HasDatabaseName("ix_characters_scan2_scan1");

            e.Property(c => c.CharacterId)
                .IsRequired();

            e.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(c => c.FoundInScan1)
                .IsRequired()
                .HasDefaultValue(false);

            e.Property(c => c.FoundInScan2)
                .IsRequired()
                .HasDefaultValue(false);

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

        #region CharacterActions

        modelBuilder.Entity<CharacterAction>(e =>
        {
            e.HasKey(ca => ca.CharacterActionId);
            e.HasIndex(ca => ca.CharacterName);
            e.HasIndex(ca => ca.IsOnline);
            e.HasIndex(ca => ca.WorldScanId);
            e.HasIndex(ca => ca.WorldId);
            e.HasIndex(ca => ca.LogoutOrLoginDate);
            e.HasIndex(ca => new { ca.IsOnline, ca.CharacterName })
                .HasDatabaseName("ix_character_actions_is_online_character_name");

            e.Property(ca => ca.CharacterActionId)
                .IsRequired();

            e.Property(ca => ca.CharacterName)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(ca => ca.WorldScanId)
                .IsRequired();

            e.Property(ca => ca.IsOnline)
                .IsRequired();

            e.Property(ca => ca.WorldId)
                .IsRequired();

            e.Property(ca => ca.LogoutOrLoginDate)
                .IsRequired();
        });

        #endregion

        #region CharacterCorrelations

        modelBuilder.Entity<CharacterCorrelation>(e =>
        {
            e.HasKey(cc => cc.CorrelationId);
            e.HasIndex(cc => cc.LogoutCharacterId);
            e.HasIndex(cc => cc.LoginCharacterId);
            e.HasIndex(cc => cc.NumberOfMatches);
            e.HasIndex(cc => new { cc.LoginCharacterId, cc.LogoutCharacterId })
                .HasDatabaseName("ix_correlations_login_character_id_logout_character_id");
            e.HasIndex(cc => new { cc.LogoutCharacterId, cc.LoginCharacterId })
                .HasDatabaseName("ix_correlations_logout_character_id_login_character_id");

            e.Property(cc => cc.CorrelationId)
                .IsRequired();

            e.Property(cc => cc.LogoutCharacterId)
                .IsRequired();

            e.Property(cc => cc.LoginCharacterId)
                .IsRequired();

            e.Property(cc => cc.NumberOfMatches)
                .IsRequired();

            e.Property(cc => cc.CreateDate)
                .IsRequired()
                .HasDefaultValue(new DateOnly(2022, 12, 06));

            e.Property(cc => cc.LastMatchDate)
                .IsRequired()
                .HasDefaultValue(new DateOnly(2022, 12, 06));
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
            e.HasIndex(oc => oc.Name);
            e.HasIndex(oc => oc.WorldName);
            e.HasKey(oc => oc.Name);

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