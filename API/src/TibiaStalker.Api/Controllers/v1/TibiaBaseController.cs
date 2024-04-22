using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TibiaStalker.Infrastructure.Configuration;

namespace TibiaStalker.Api.Controllers.v1;

[ApiController]
[EnableRateLimiting(ConfigurationConstants.GlobalRateLimiting)]
[Route("api/tibia-stalker/v{version:apiVersion}/[controller]")]
public class TibiaBaseController : ControllerBase
{
}