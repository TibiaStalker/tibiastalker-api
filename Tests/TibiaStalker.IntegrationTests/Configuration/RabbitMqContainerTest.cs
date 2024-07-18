using Testcontainers.RabbitMq;

namespace TibiaStalker.IntegrationTests.Configuration;

public sealed class RabbitMqContainerTest
{
	public readonly RabbitMqContainer Container;

	public RabbitMqContainerTest()
	{
	 Container = new RabbitMqBuilder()
			.WithImage("rabbitmq:3-management")
			.WithUsername("guest")
			.WithPassword("guest")
			.WithEnvironment("RABBITMQ_DEFAULT_VHOST", "tibia-stalker")
			.WithPortBinding(15672, true)
			.WithPortBinding(5672, true)
			.Build();
	}
}