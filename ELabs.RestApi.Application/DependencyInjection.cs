using ELabs.RestApi.Application.Assembly;
using ELabs.RestApi.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ELabs.RestApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);


        // ------------------------------------------------------
        // PIPELINE BEHAVIOR ORDER (outermost -> innermost)
        //
        // MediatR wraps behaviors in registration order, so the FIRST registered
        // behavior is the OUTERMOST (runs first on the way in, last on the way out).
        //
        //  ┌ Logging        ← times the full operation, hit or miss
        //  │ ┌ Audit        ← records SUCCESS / NOT_FOUND / FAILURE (snapshot taken after next())
        //  │ │ ┌ Validation ← rejects bad input, also before a cached response is served
        //  │ │ │ ┌ Caching  ← returns a cached response, or runs the handler and caches it
        //  │ │ │ │  Handler
        //
        // Because Caching is innermost, cache hits are logged and audited by the same code as
        // misses. An audit-write failure therefore fails the request on a hit, as on a miss.
        // ------------------------------------------------------
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}
