using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class TrackedCharacterConfiguration : IEntityTypeConfiguration<TrackedCharacter>
{
	public void Configure(EntityTypeBuilder<TrackedCharacter> builder)
	{
		builder.HasIndex(tc => tc.Name);
		builder.HasIndex(tc => tc.WorldName);
		builder.HasKey(tc => new { tc.Name, tc.ConnectionId });

		builder.Property(tc => tc.Name)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(tc => tc.WorldName)
			.HasMaxLength(20)
			.IsRequired();

		builder.Property(tc => tc.ConnectionId)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(tc => tc.StartTrackDateTime)
			.IsRequired();
	}
}