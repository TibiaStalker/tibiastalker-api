﻿using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TibiaStalker.Application.Queries.World;

namespace TibiaStalker.Api.Controllers.v1;

[ApiVersion("1.0")]
public class WorldsController : TibiaBaseController
{
    private readonly IMediator _mediator;

    public WorldsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get filtered worlds.
    /// </summary>
    /// <param name="isAvailable">If "true" than return all active worlds at this moment in applocation.</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Worlds count, names, urls and availability.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorlds([FromQuery] bool? isAvailable, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetActiveWorldsQuery(isAvailable), ct);
        return Ok(result);
    }
}
