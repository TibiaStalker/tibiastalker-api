using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Configuration;

namespace WorldScanSeeder.Decorators;

public interface IScanSeederLogDecorator : IParameterizedLoggerDecorator<World>
{
}