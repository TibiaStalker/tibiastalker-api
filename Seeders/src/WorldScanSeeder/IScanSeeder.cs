using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;

namespace WorldScanSeeder;

public interface IScanSeeder : ISeeder<World>
{
    List<World> AvailableWorlds { get; }
    Task SetProperties();
}