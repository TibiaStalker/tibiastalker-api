using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
	public void Configure(EntityTypeBuilder<Character> builder)
	{
		builder.HasKey(c => c.CharacterId);
		builder.HasIndex(c => c.Name);
		builder.HasIndex(c => c.WorldId);
		builder.HasIndex(c => c.VerifiedDate);
		builder.HasIndex(c => c.TradedDate);

		builder.Property(c => c.CharacterId)
			.IsRequired();

		builder.Property(c => c.Name)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(c => c.VerifiedDate)
			.IsRequired()
			.HasDefaultValue(new DateOnly(2001, 01, 01));

		builder.Property(c => c.TradedDate)
			.IsRequired()
			.HasDefaultValue(new DateOnly(2001, 01, 01));

		builder.HasMany(c => c.LogoutCharacterCorrelations)
			.WithOne(cc => cc.LogoutCharacter)
			.HasForeignKey(cc => cc.LogoutCharacterId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(c => c.LoginCharacterCorrelations)
			.WithOne(cc => cc.LoginCharacter)
			.HasForeignKey(cc => cc.LoginCharacterId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(c => c.DeleteApproachNumber)
			.IsRequired()
			.HasDefaultValue(0);
	}
}