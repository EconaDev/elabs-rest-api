using ELabs.RestApi.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ELabs.RestApi.Infrastructure.Services.System;

public class EnvironmentService(IHostEnvironment environment) : IEnvironmentService
{
    public bool IsDevelopment() => environment.IsDevelopment();
    public bool IsStaging() => environment.IsStaging();
    public bool IsProduction() => environment.IsProduction();
}
