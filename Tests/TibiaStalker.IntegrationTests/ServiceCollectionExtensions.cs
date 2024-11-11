using System.Reflection;
using System.Runtime.CompilerServices;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

namespace TibiaStalker.IntegrationTests;

public static class ServiceCollectionExtensions
{
    public static void AddProjectServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        var allTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            allTypes.AddRange(assembly.GetTypes());
        }

        var executingAssembly = Assembly.GetExecutingAssembly();
        allTypes.AddRange(executingAssembly.GetTypes());

        var types = allTypes
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(t => !t.IsGenericTypeDefinition)
            .Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
            .Select(t => new
            {
                Service = t.GetInterfaces().FirstOrDefault(i => i.IsPublic && !i.IsGenericTypeDefinition),
                Implementation = t
            })
            .Where(t => t.Service != null);

        foreach (var type in types)
        {
            if (typeof(ITestDatabaseSeeder).IsAssignableFrom(type.Service))
            {
                services.AddTransient(type.Implementation);
                Console.WriteLine($"Added ITestDatabaseSeeder: {type.Service.Name} - {type.Implementation.Name}");
            }
            else if (typeof(IAsyncStateMachine).IsAssignableFrom(type.Service))
            {
                Console.WriteLine($"(IAsyncStateMachine): {type.Service.Name} - {type.Implementation.Name}");
            }
            else
            {
                Console.WriteLine($"(Else): {type.Service!.Name} - {type.Implementation.Name}");
                services.AddScoped(type.Service, type.Implementation);
            }
        }
    }
}