using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ELabs.RestApi.WebAPI.Controllers.Forms;

/// <summary>
/// Controller for Northwind Forms related operations.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "forms")]
[Route("api/northwind")]
public class FormsController : ControllerBase
{
    /// <summary>
    /// Verifies the connector path (gateway, Kerberos, service account). Does not touch the Northwind database.
    /// </summary>
    /// <returns></returns>
    [HttpGet("status")]
    [SwaggerOperation(OperationId = "FormsGetStatus")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetStatus() => Ok(new StatusResponse("forms", "ok"));
}
