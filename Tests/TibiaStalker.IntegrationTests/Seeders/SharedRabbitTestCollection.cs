using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.Seeders;

[CollectionDefinition(TestCollections.RabbitCollection, DisableParallelization = true)]
public class SharedRabbitTestCollection : ICollectionFixture<TibiaSeederFactory>
{
}