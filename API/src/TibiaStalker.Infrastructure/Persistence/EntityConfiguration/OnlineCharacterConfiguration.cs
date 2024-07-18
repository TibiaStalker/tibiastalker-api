using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TibiaStalker.Domain.Entities;

namespace TibiaStalker.Infrastructure.Persistence.EntityConfiguration;

public class OnlineCharacterConfiguration : IEntityTypeConfiguration<OnlineCharacter>
{
	public void Configure(EntityTypeBuilder<OnlineCharacter> builder)
	{
		builder.HasKey(oc => new {oc.Name, oc.OnlineDateTime});
		builder.HasIndex(oc => oc.WorldName);

		builder.Property(oc => oc.Name)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(oc => oc.WorldName)
			.HasMaxLength(20)
			.IsRequired();

		builder.Property(oc => oc.OnlineDateTime)
			.IsRequired();
	}
}