using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Configuration;

namespace WorldScanAnalyserSubscriber.Events.Decorators;

public interface IAnalyserLogDecorator : IParameterizedLoggerDecorator<WorldScan[]>, ILoggerDecorator<WorldScan[]>
{
}