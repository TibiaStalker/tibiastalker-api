﻿using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Configuration;

namespace WorldScanAnalyser.Decorators;

public interface IAnalyserLogDecorator : IParameterizedLoggerDecorator<List<WorldScan>>, ILoggerDecorator<List<WorldScan>>
{
}