/*
 - Application Startup Configuration
 - This is the entry point of our Azure Function app
 - It configures services, dependency injection, and middleware
 */
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VisitorsReg.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Configure Application Insights for monitoring and telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        /*
         - Dependency Injection Configuration
         - Here we register our services so they can be injected into our functions
         */
        services.AddScoped<PostgresService>(provider =>
        {
            // Get logger for PostgresService
            var logger = provider.GetRequiredService<ILogger<PostgresService>>();
            
            // Get connection string from environment variables
            // In Azure: This comes from Application Settings
            // Locally: This comes from local.settings.json
            var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("POSTGRESQL_CONNECTION_STRING environment variable is not set");
            }
            
            return new PostgresService(connectionString, logger);
        });
    })
    .Build();

// Run the application
host.Run();
