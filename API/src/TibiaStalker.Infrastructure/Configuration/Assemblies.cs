using System.Reflection;
using TibiaStalker.Application.Configuration;

namespace TibiaStalker.Infrastructure.Configuration
{
    public static class Assemblies
    {
        public static Assembly ApplicationAssembly { get; } = typeof(AssembliesHook).Assembly;
    }
}