namespace TibiaStalker.Application.Interfaces;

public interface ISeederService
{
    Task Seed();
}

public interface ISeeder<T>
{
    Task Seed(T worldScans);
}
