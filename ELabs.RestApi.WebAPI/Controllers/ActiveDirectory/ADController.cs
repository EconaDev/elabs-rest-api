using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ELabs.RestApi.WebAPI.Controllers.ActiveDirectory;

/// <summary>
/// Controller for Active Directory related operations.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "directory")]
[Route("api/ad")]
public class AdController : ControllerBase
{
    /// <summary>
    /// Verifies the connector path (gateway, Kerberos, service account). Does not touch Active Directory.
    /// </summary>
    /// <returns></returns>
    [HttpGet("status")]
    [SwaggerOperation(OperationId = "AdGetStatus")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetStatus() => Ok(new StatusResponse("directory", "ok"));
}
