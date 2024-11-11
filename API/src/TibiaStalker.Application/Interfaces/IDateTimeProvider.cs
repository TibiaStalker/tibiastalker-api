namespace TibiaStalker.Application.Interfaces;

public interface IDateTimeProvider
{
     DateTime DateTimeUtcNow { get; }
     DateOnly DateOnlyUtcNow { get; }
}
