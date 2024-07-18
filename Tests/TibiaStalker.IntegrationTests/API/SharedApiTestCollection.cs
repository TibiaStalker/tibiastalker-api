using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API;

[CollectionDefinition(TestCollections.ApiCollection, DisableParallelization = true)]
public class SharedApiTestCollection : ICollectionFixture<TibiaApiFactory>
{
}