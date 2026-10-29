using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ELabs.RestApi.WebAPI.Controllers.Admin;

/// <summary>
/// Controller for Northwind Admin related operations.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "admin")]
[Route("api/northwind/admin")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Verifies the connector path (gateway, Kerberos, service account). Does not touch the Northwind database.
    /// </summary>
    /// <returns></returns>
    [HttpGet("status")]
    [SwaggerOperation(OperationId = "AdminGetStatus")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetStatus() => Ok(new StatusResponse("admin", "ok"));
}
