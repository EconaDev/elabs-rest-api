using ELabs.RestApi.Application.Features.Customers.Queries.GetCustomerByCustomerKey;
using ELabs.RestApi.WebAPI.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ELabs.RestApi.WebAPI.Controllers.Reference;

/// <summary>
/// Handles HTTP requests for customer resources sourced from the AWDW (AdventureWorksDW).
/// All routes are prefixed with 'api/awdw/customers'.
/// </summary>
/// <param name="mediator"></param>
[ApiController]
[ApiExplorerSettings(GroupName = "reference")]
[Route("api/awdw/customers")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves the customer associated with the specified customer key.
    /// </summary>
    /// <param name="customerKey">The AWDW customer key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The customer if found; otherwise NotFound.</returns>
    [HttpGet("{customerKey:int}")]
    [SwaggerOperation(OperationId = "AwdwGetCustomerByCustomerKey")]
    [ProducesResponseType(typeof(GetCustomerByCustomerKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        int customerKey,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCustomerByCustomerKeyQuery
            {
                CustomerKey = customerKey
            },
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}
