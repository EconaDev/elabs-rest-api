using ELabs.RestApi.Application.Features.Stores.Queries.GetStoreByBusinessEntityId;
using ELabs.RestApi.WebAPI.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ELabs.RestApi.WebAPI.Controllers.Reference;

/// <summary>
/// Handles HTTP requests related to store resources, such as retrieving store information by business
/// entity ID.
/// </summary>
/// <remarks>This controller provides endpoints for accessing store data via RESTful APIs. All routes are
/// prefixed with 'api/aw/stores'.</remarks>
/// <remarks>
/// Gets store information by business entity ID.
/// </remarks>
/// <param name="mediator"></param>
[ApiController]
[ApiExplorerSettings(GroupName = "reference")]
[Route("api/aw/stores")]
public class StoresController(IMediator mediator) : ControllerBase
{

    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves the store details associated with the specified business entity ID.
    /// </summary>
    /// <param name="businessEntityId">The business entity ID of the store to retrieve. Must be a valid integer representing an existing store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An IActionResult containing the store details if found; otherwise, a NotFound result if no store
    /// matches the specified business entity ID.</returns>
    [HttpGet("{businessEntityId:int}")]
    [SwaggerOperation(OperationId = "AwGetStoreByBusinessEntityId")]
    [ProducesResponseType(typeof(GetStoreByBusinessEntityIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        int businessEntityId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStoreByBusinessEntityIdQuery
            {
                BusinessEntityId = businessEntityId
            },
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

}
