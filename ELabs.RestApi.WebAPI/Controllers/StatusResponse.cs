namespace ELabs.RestApi.WebAPI.Controllers;

/// <summary>
/// This code defines a record named StatusResponse
/// </summary>
/// <param name="Connector"></param>
/// <param name="Status"></param>
public sealed record StatusResponse(string Connector, string Status);
