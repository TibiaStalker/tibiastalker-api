using TibiaStalker.Application.Interfaces;

namespace TibiaStalker.Infrastructure.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime DateTimeUtcNow => DateTime.UtcNow;
    public DateOnly DateOnlyUtcNow => DateOnly.FromDateTime(DateTimeUtcNow);
}