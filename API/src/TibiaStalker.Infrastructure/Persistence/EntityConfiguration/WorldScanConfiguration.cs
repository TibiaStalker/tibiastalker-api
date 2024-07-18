using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class WorldScanConfiguration : IEntityTypeConfiguration<WorldScan>
{
	public void Configure(EntityTypeBuilder<WorldScan> builder)
	{
		builder.HasKey(ws => ws.WorldScanId);
		builder.HasIndex(ws => ws.WorldId);
		builder.HasIndex(ws => ws.ScanCreateDateTime);
		builder.HasIndex(ws => ws.IsDeleted);
		builder.HasIndex(ws => new { ws.WorldId, ws.IsDeleted })
			.HasDatabaseName("ix_world_scan_id_world_id_is_deleted");
		builder.HasIndex(ws => new { ws.WorldId, ws.IsDeleted, ws.ScanCreateDateTime })
			.HasDatabaseName("ix_world_scan_world_id_is_deleted_scan_date_time");

		builder.Property(ws => ws.WorldScanId)
			.IsRequired();

		builder.Property(ws => ws.CharactersOnline)
			.IsRequired();

		builder.Property(ws => ws.WorldId)
			.IsRequired();

		builder.Property(ws => ws.ScanCreateDateTime)
			.IsRequired();

		builder.Property(ws => ws.IsDeleted)
			.IsRequired()
			.HasDefaultValue(false);
	}
}