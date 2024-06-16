using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyserSubscriber.Events;

public interface IAnalyser : ISeeder<WorldScan[]>
{
}