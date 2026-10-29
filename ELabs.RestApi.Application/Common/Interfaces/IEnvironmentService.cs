namespace ELabs.RestApi.Application.Common.Interfaces;

public interface IEnvironmentService
{
    bool IsDevelopment();
    bool IsStaging();
    bool IsProduction();
}
