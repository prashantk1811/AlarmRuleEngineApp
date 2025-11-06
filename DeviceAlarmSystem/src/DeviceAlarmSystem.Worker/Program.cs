using DeviceAlarmSystem.Infrastructure;
using DeviceAlarmSystem.RuleEngine.Extensions;
using DeviceAlarmSystem.Worker;
using DeviceAlarmSystem.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Load configuration
        var configuration = context.Configuration;

        // Register infrastructure & rule engine
        services.AddInfrastructureServices(configuration);
        services.AddRuleEngine();

        // Add the worker & monitor service
        services.AddHostedService<Worker>();
        services.AddScoped<ParameterMonitorService>();

        services.AddLogging(config =>
        {
            config.AddConsole();
        });
    })
    .Build();

await host.RunAsync();
