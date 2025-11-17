using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Core.Models;
using DeviceAlarmSystem.DeviceMonitoring;
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

        // Register MQTT configuration and monitor
        services.Configure<MqttConfiguration>(configuration.GetSection("MqttConfiguration"));
        services.AddSingleton<IMqttMonitor, MqttMonitor>();
        services.AddSingleton<IDeviceParameterValueProvider>(sp => sp.GetRequiredService<IMqttMonitor>() as IDeviceParameterValueProvider);

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
