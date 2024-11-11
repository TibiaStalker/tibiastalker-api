using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.Seeders;

[CollectionDefinition(TestCollections.SeederCollection, DisableParallelization = true)]
public class SharedSeederTestCollection : ICollectionFixture<TibiaSeederFactory>
{
}