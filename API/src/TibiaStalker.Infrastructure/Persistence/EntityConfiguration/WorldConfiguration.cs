using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class WorldConfiguration : IEntityTypeConfiguration<World>
{
	public void Configure(EntityTypeBuilder<World> builder)
	{
			builder.Property(w => w.Name)
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(w => w.Url)
				.HasMaxLength(200)
				.IsRequired();

			builder.Property(w => w.IsAvailable)
				.IsRequired();

			builder.HasMany(w => w.WorldScans)
				.WithOne(ws => ws.World)
				.HasForeignKey(ws => ws.WorldId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasMany(w => w.Characters)
				.WithOne(c => c.World)
				.HasForeignKey(c => c.WorldId)
				.OnDelete(DeleteBehavior.NoAction);
	}
}