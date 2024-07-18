using Testcontainers.PostgreSql;

namespace TibiaStalker.IntegrationTests.Configuration;

public sealed class PostgreSqlContainerTest
{
	public readonly PostgreSqlContainer Container;

	public PostgreSqlContainerTest()
	{
		Container = new PostgreSqlBuilder()
			.WithImage("postgres:15-alpine")
			.Build();
	}
}