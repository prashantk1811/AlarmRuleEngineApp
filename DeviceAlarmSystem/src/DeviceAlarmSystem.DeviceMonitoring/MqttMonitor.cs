using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.DeviceMonitoring
{
    public class MqttMonitor : IMqttMonitor, IDeviceParameterValueProvider
    {
        private readonly IMqttClient _client;
        private readonly MqttConfiguration _config;
        private readonly ILogger<MqttMonitor> _logger;
        private readonly ConcurrentDictionary<string, double> _parameterValues;
        private bool _isConnected;

        public MqttMonitor(IOptions<MqttConfiguration> config, ILogger<MqttMonitor> logger)
        {
            _config = config.Value;
            _logger = logger;
            _parameterValues = new ConcurrentDictionary<string, double>();
            
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            
            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        public async Task StartMonitoringAsync()
        {
            try
            {
                if (_isConnected)
                {
                    _logger.LogWarning("MQTT client is already connected.");
                    return;
                }

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_config.BrokerHost, _config.BrokerPort)
                    .WithCleanSession(_config.CleanSession)
                    .WithClientId($"DeviceAlarmSystem_{_config.SolutionId}")
                    .Build();

                await _client.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start MQTT monitoring");
                throw;
            }
        }

        public async Task StopMonitoringAsync()
        {
            try
            {
                if (_client.IsConnected)
                {
                    await _client.DisconnectAsync();
                    _logger.LogInformation("MQTT monitoring stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MQTT monitoring");
            }
        }

        public async Task PublishAlarmAsync(Guid deviceId, Guid parameterId, Guid alarmId, string alarmData)
        {
            try
            {
                var topic = MqttTopics.BuildAlarmTopic(_config.SolutionId, deviceId, parameterId, alarmId);
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(alarmData)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _client.PublishAsync(message);
                _logger.LogInformation("Published alarm to topic: {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish alarm for AlarmId: {AlarmId}", alarmId);
                throw;
            }
        }

        public async Task PublishAlarmAcknowledgementAsync(Guid deviceId, Guid parameterId, Guid alarmId, string ackData)
        {
            try
            {
                var topic = MqttTopics.BuildAlarmAckTopic(_config.SolutionId, deviceId, parameterId, alarmId);
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(ackData)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _client.PublishAsync(message);
                _logger.LogInformation("Published alarm acknowledgement to topic: {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish alarm acknowledgement for AlarmId: {AlarmId}", alarmId);
                throw;
            }
        }

        public double? GetCurrentValue(string deviceId, string parameterName)
        {
            var key = $"{deviceId}:{parameterName}";
            return _parameterValues.TryGetValue(key, out var value) ? value : null;
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _isConnected = true;
            _logger.LogInformation("MQTT client connected to broker at {Host}:{Port}", _config.BrokerHost, _config.BrokerPort);

            try
            {
                // Subscribe to all current value topics for this solution
                var topic = MqttTopics.BuildAllCurrentValuesTopic(_config.SolutionId);
                await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                _logger.LogInformation("Subscribed to topic: {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to topics");
            }
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _isConnected = false;
            _logger.LogWarning("MQTT client disconnected. Reason: {Reason}", args.Reason);

            // Auto-reconnect logic
            if (!args.ClientWasConnected)
            {
                _logger.LogInformation("Attempting to reconnect in {Delay}ms...", _config.ReconnectDelay);
                await Task.Delay(_config.ReconnectDelay);
                
                try
                {
                    await StartMonitoringAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reconnection attempt failed");
                }
            }
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var topic = args.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

                _logger.LogDebug("Received message on topic: {Topic}, Payload: {Payload}", topic, payload);

                // Parse topic to extract device and parameter IDs
                var (deviceId, parameterId) = MqttTopics.ParseCurrentValueTopic(topic);
                
                if (deviceId.HasValue && parameterId.HasValue)
                {
                    // Try to parse as JSON first
                    if (TryParseJsonValue(payload, out var jsonValue))
                    {
                        var key = $"{deviceId.Value}:{parameterId.Value}";
                        _parameterValues.AddOrUpdate(key, jsonValue.Value, (k, v) => jsonValue.Value);
                        
                        _logger.LogInformation("Updated parameter value: DeviceId={DeviceId}, ParameterId={ParameterId}, Value={Value}",
                            deviceId.Value, parameterId.Value, jsonValue.Value);
                    }
                    // Fallback to simple double parse
                    else if (double.TryParse(payload, out var value))
                    {
                        var key = $"{deviceId.Value}:{parameterId.Value}";
                        _parameterValues.AddOrUpdate(key, value, (k, v) => value);
                        
                        _logger.LogInformation("Updated parameter value: DeviceId={DeviceId}, ParameterId={ParameterId}, Value={Value}",
                            deviceId.Value, parameterId.Value, value);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse value from payload: {Payload}", payload);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to parse topic: {Topic}", topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }

            await Task.CompletedTask;
        }

        private bool TryParseJsonValue(string payload, out CurrentValueMessage value)
        {
            try
            {
                value = JsonSerializer.Deserialize<CurrentValueMessage>(payload);
                return value != null;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
