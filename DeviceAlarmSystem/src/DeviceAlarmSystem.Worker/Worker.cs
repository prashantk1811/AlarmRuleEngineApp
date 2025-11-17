using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _services;
        private readonly IMqttMonitor _mqttMonitor;

        public Worker(ILogger<Worker> logger, IServiceProvider services, IMqttMonitor mqttMonitor)
        {
            _logger = logger;
            _services = services;
            _mqttMonitor = mqttMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Device Alarm Rule Evaluation Worker started.");

            // Start MQTT monitoring
            try
            {
                await _mqttMonitor.StartMonitoringAsync();
                _logger.LogInformation("MQTT monitoring started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start MQTT monitoring");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var monitorService = scope.ServiceProvider.GetRequiredService<ParameterMonitorService>();

                    await monitorService.ProcessParametersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during rule evaluation cycle.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            // Stop MQTT monitoring on shutdown
            try
            {
                await _mqttMonitor.StopMonitoringAsync();
                _logger.LogInformation("MQTT monitoring stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MQTT monitoring");
            }
        }
    }
}
