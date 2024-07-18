using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class CharacterCorrelationConfiguration : IEntityTypeConfiguration<CharacterCorrelation>
{
	public void Configure(EntityTypeBuilder<CharacterCorrelation> builder)
	{
		builder.HasKey(cc => cc.CorrelationId);
		builder.HasIndex(cc => cc.LogoutCharacterId);
		builder.HasIndex(cc => cc.LoginCharacterId);
		builder.HasIndex(cc => cc.NumberOfMatches);

		builder.Property(cc => cc.CorrelationId)
			.IsRequired();

		builder.Property(cc => cc.LogoutCharacterId)
			.IsRequired();

		builder.Property(cc => cc.LoginCharacterId)
			.IsRequired();

		builder.Property(cc => cc.NumberOfMatches)
			.IsRequired();

		builder.Property(cc => cc.CreateDate)
			.IsRequired();

		builder.Property(cc => cc.LastMatchDate)
			.IsRequired();
	}
}