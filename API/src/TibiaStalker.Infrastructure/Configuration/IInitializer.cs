namespace TibiaStalker.Infrastructure.Configuration;

public interface IInitializer
{
    int? Order { get; }
    Task Initialize();
}
