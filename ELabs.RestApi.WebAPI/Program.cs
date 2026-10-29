using ELabs.RestApi.Application;
using ELabs.RestApi.Infrastructure;
using ELabs.RestApi.WebAPI.Middleware;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Authorization;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

// ------------------------------------------------------------
// BOOTSTRAP LOGGER (replaced by full config below)
// ------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();

    // --------------------------------------------------------
    // SERILOG
    // --------------------------------------------------------
    builder.Host.UseSerilog((context, services, loggerConfig) =>
    {
        var connectionString = context.Configuration.GetConnectionString("ELabs_Northwind")
            ?? throw new InvalidOperationException("Missing connection string: ELabs_Northwind");

        var columnOptions = new ColumnOptions
        {
            Store =
            [
                StandardColumn.Id,
                StandardColumn.Message,
                StandardColumn.MessageTemplate,
                StandardColumn.Level,
                StandardColumn.TimeStamp,
                StandardColumn.Exception,
                StandardColumn.Properties,
                StandardColumn.LogEvent
            ],

            AdditionalColumns =
            [
                new("CorrelationId", SqlDbType.NVarChar, dataLength: 100),
                new("UserEmail",     SqlDbType.NVarChar, dataLength: 200),
                new("UserName",      SqlDbType.NVarChar, dataLength: 200),
                new("FormName",      SqlDbType.NVarChar, dataLength: 200),
                new("FormSection",   SqlDbType.NVarChar, dataLength: 200),
                new("ClientIP",      SqlDbType.NVarChar, dataLength: 100),
                new("UserAgent",     SqlDbType.NVarChar, dataLength: 500),
                new("RequestName",   SqlDbType.NVarChar, dataLength: 200),
                new("ElapsedUs",     SqlDbType.BigInt),
                new("AfterJson", SqlDbType.NVarChar, dataLength: -1),
                new("ValidationErrorsJson", SqlDbType.NVarChar, dataLength: -1)
            ]
        };

        var sinkOptions = new MSSqlServerSinkOptions
        {
            TableName = builder.Configuration["Serilog:Properties:TableName"] ?? throw new InvalidOperationException("Serilog:Properties:TableName is not configured."),
            AutoCreateSqlTable = false
        };

        loggerConfig
            .ReadFrom.Configuration(context.Configuration)

            // reduce EF noise
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)

            // enrichers
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()

            // console (dev visibility)
            .WriteTo.Console(
                restrictedToMinimumLevel: context.HostingEnvironment.IsDevelopment()
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information)

            // fallback channel, independent of SQL: Error and above, so failures (e.g. audit writes)
            // stay visible when the Northwind SQL sink is unavailable. The app pool identity needs
            // write access to the logs folder.
            .WriteTo.File(
                path: Path.Combine(AppContext.BaseDirectory, "logs", "fallback-.log"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)

            // SQL sink
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: sinkOptions,
                columnOptions: columnOptions,
                restrictedToMinimumLevel: context.HostingEnvironment.IsProduction()
                    ? LogEventLevel.Information
                    : LogEventLevel.Debug
            );
    });

    // --------------------------------------------------------
    // SERVICES
    // --------------------------------------------------------
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();

    // --------------------------------------------------------
    // AUTHENTICATION / AUTHORIZATION
    // --------------------------------------------------------
    string allowedGatewayAccount = builder.Configuration["Auth:AllowedGatewayAccount"] ?? throw new InvalidOperationException("Auth:AllowedGatewayAccount is not configured");

    builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ELabsGatewayAccount", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                //Local/dev debugging
                policy.RequireAuthenticatedUser();
            }
            else
            {
                policy.RequireAssertion(context =>
                context.User.Identity is { IsAuthenticated: true, Name: string name } &&
                name.Equals(allowedGatewayAccount, StringComparison.OrdinalIgnoreCase));
            }
        });
    });

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new AuthorizeFilter("ELabsGatewayAccount"));
    });



    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // --------------------------------------------------------
    // HEALTH CHECKS
    // --------------------------------------------------------
    var awConn = builder.Configuration.GetConnectionString("ELabs_AWConnection")
        ?? throw new InvalidOperationException("Missing connection string: ELabs_AWConnection");

    builder.Services.AddHealthChecks()
        .AddSqlServer(awConn);

    // --------------------------------------------------------
    // SWAGGER
    // --------------------------------------------------------
    (string Name, string Title)[] swaggerDocs =
    [
        ("reference", "ELabs Reference Data"),
        ("forms", "ELabs Forms"),
        ("admin", "ELabs Admin"),
        ("directory", "ELabs Active Directory"),
    ];

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {

        foreach ((string name, string title) in swaggerDocs)

            options.SwaggerDoc(name, new OpenApiInfo { Title = title, Version = "v1" });

        //Strict: ungrouped actions land in NO document.
        options.DocInclusionPredicate((string doc, ApiDescription api) => api.GroupName == doc);
        options.EnableAnnotations();

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ELabs API",
            Version = "v1",
            Description = "ELabs REST API"
        });

        string xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    var app = builder.Build();

    //Console.WriteLine($"Final Environment: {app.Environment.EnvironmentName}");

    // --------------------------------------------------------
    // MIDDLEWARE PIPELINE (order matters)
    //
    // 1. CorrelationId      - sets HttpContext.TraceIdentifier to X-Correlation-ID
    // 2. RequestContext     - builds IRequestContext from TraceIdentifier + headers
    // 3. Exception          - catches anything thrown by downstream middleware/handlers
    //
    // No UseForwardedHeaders(): no proxy sits in front of IIS, so X-Forwarded-For is untrusted.
    // If one is added, configure ForwardedHeadersOptions with KnownProxies and call
    // UseForwardedHeaders() first in this list.
    // --------------------------------------------------------
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestContextMiddleware>();
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(c => c.SerializeAsV2 = true);
        app.UseSwaggerUI(options =>
        {
            foreach ((string name, string title) in swaggerDocs)
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", title);
            options.RoutePrefix = string.Empty;
        });
    }

    app.MapHealthChecks("/health");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
