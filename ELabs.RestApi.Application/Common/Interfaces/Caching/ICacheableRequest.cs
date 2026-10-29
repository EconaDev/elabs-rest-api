namespace ELabs.RestApi.Application.Common.Interfaces.Caching;

public interface ICacheableRequest
{
    string CacheKey { get; }

    TimeSpan? AbsoluteExpirationRelativeToNow { get; }

    TimeSpan? SlidingExpiration { get; }
}
