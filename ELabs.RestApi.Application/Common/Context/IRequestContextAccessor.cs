
namespace ELabs.RestApi.Application.Common.Context;

public interface IRequestContextAccessor
{
    IRequestContext Current { get; }
}
