using System.Reflection;

namespace TibiaStalker.IntegrationTests.Configuration
{
    public static class TestAssemblies
    {
        public static Assembly[] ApplicationsAssembly { get; } = new[]
        {
            typeof(ChangeNameDetector.Configuration.AssembliesHook).Assembly,
            typeof(ChangeNameDetectorSubscriber.Configuration.AssembliesHook).Assembly,
            typeof(DbCleaner.Configuration.AssembliesHook).Assembly,
            typeof(WorldScanAnalyser.Configuration.AssembliesHook).Assembly,
            typeof(WorldScanAnalyserSubscriber.Configuration.AssembliesHook).Assembly,
            typeof(WorldScanSeeder.Configuration.AssembliesHook).Assembly,
            typeof(WorldSeeder.Configuration.AssembliesHook).Assembly,
        };
    }
}