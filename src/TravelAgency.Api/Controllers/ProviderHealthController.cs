using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/providers/health")]
public class ProviderHealthController(IProviderHealthService providerHealth) : ControllerBase
{
	[HttpGet]
	public IActionResult Get()
	{
		return Ok(providerHealth.GetCurrent());
	}
}
