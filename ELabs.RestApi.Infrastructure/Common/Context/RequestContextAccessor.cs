using ELabs.RestApi.Application.Common.Context;
using Microsoft.AspNetCore.Http;

namespace ELabs.RestApi.Infrastructure.Common.Context;

public class RequestContextAccessor(IHttpContextAccessor http)
    : IRequestContextAccessor
{
    private const string Key = "REQUEST_CONTEXT";

    public IRequestContext Current
    {
        get
        {
            var ctx = http.HttpContext
                ?? throw new InvalidOperationException(
                    "HttpContext is null. Ensure this is called within an HTTP request.");

            if (ctx.Items.TryGetValue(Key, out var existing) &&
                existing is IRequestContext requestContext)
            {
                return requestContext;
            }

            throw new InvalidOperationException(
                "RequestContext not found. Ensure RequestContextMiddleware is registered BEFORE MediatR pipeline.");
        }
    }
}
