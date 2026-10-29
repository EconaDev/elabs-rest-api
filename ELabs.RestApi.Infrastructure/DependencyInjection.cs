using ELabs.RestApi.Application.Common.Context;
using ELabs.RestApi.Application.Common.Interfaces;
using ELabs.RestApi.Application.Common.Interfaces.Audit;
using ELabs.RestApi.Application.Features.Customers.Repositories;
using ELabs.RestApi.Application.Features.Stores.Repositories;

using ELabs.RestApi.Infrastructure.Common.Context;
using ELabs.RestApi.Infrastructure.External.AWDW;
using ELabs.RestApi.Infrastructure.External.AWDW.Repositories;
using ELabs.RestApi.Infrastructure.External.AW;
using ELabs.RestApi.Infrastructure.External.AW.Repositories;
using ELabs.RestApi.Infrastructure.Persistence.Northwind;
using ELabs.RestApi.Infrastructure.Services.Audit;
using ELabs.RestApi.Infrastructure.Services.System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ELabs.RestApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ------------------------------------------------------
        // CONNECTION STRINGS
        // ------------------------------------------------------
        var northwindConn = configuration.GetConnectionString("ELabs_Northwind")
            ?? throw new InvalidOperationException("Missing connection string: ELabs_Northwind");

        var awConn = configuration.GetConnectionString("ELabs_AWConnection")
            ?? throw new InvalidOperationException("Missing connection string: ELabs_AWConnection");

        var awdwConn = configuration.GetConnectionString("ELabs_AWDWConnection")
            ?? throw new InvalidOperationException("Missing connection string: ELabs_AWDWConnection");

        // ------------------------------------------------------
        // CORE INFRASTRUCTURE
        // ------------------------------------------------------
        services.AddHttpContextAccessor();

        // RequestContextAccessor reads the context that RequestContextMiddleware stamps
        // onto HttpContext.Items. IRequestContextFactory is intentionally NOT registered —
        // it was dead code (nothing called Create()). Context construction lives in the
        // middleware where it belongs.
        services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

        // ------------------------------------------------------
        // REPOSITORIES
        // ------------------------------------------------------
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // ------------------------------------------------------
        // SERVICES
        // ------------------------------------------------------
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEnvironmentService, EnvironmentService>();

        // ------------------------------------------------------
        // DB CONTEXTS
        // ------------------------------------------------------
        services.AddDbContext<NorthwindDbContext>((sp, options) =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            options.UseSqlServer(northwindConn);
            ApplyEfCoreConfig(options, env);
        });

        services.AddDbContext<AWDbContext>((sp, options) =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            options.UseSqlServer(awConn);
            ApplyEfCoreConfig(options, env);
        });

        services.AddDbContext<AWDWDbContext>((sp, options) =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            options.UseSqlServer(awdwConn);
            ApplyEfCoreConfig(options, env);
        });

        return services;
    }

    private static void ApplyEfCoreConfig(DbContextOptionsBuilder options, IHostEnvironment env)
    {
        options.LogTo(_ => { }, LogLevel.None);
        options.EnableSensitiveDataLogging(env.IsDevelopment());
        options.EnableDetailedErrors(env.IsDevelopment());
        options.EnableServiceProviderCaching();
    }
}
