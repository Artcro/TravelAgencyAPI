using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Infrastructure.Config;

namespace TravelAgency.Tests.Travel;

public class DuffelHttpClientConfigTests
{
	[Fact]
	public void Duffel_HttpClient_Uses_Configured_TimeoutSeconds()
	{
		var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
		{
			["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
			["Duffel:AccessToken"] = "token",
			["Duffel:TimeoutSeconds"] = "47"
		}).Build();

		var services = new ServiceCollection();
		services.AddInfrastructure(config);
		var sp = services.BuildServiceProvider();

		var factory = sp.GetRequiredService<IHttpClientFactory>();
		var client = factory.CreateClient("duffel");
		Assert.Equal(TimeSpan.FromSeconds(47), client.Timeout);
	}
}