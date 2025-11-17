# MQTT Integration Guide

This document describes the MQTT integration for the Device Alarm System, including topic structure, message formats, and implementation details.

## Overview

The Device Alarm System uses MQTT for real-time device monitoring and alarm notification. The system:

1. **Subscribes** to device parameter current value topics
2. **Receives** parameter updates from devices
3. **Evaluates** rules against current parameter values
4. **Publishes** alarm notifications when rules are triggered
5. **Receives** alarm acknowledgements from operators

## Topic Structure

All topics follow a hierarchical structure based on Solution ID, Device ID, Parameter ID, and Alarm ID.

### Topic Patterns

#### Current Value Topic (Device → System)
```
{solutionId}/{deviceId}/{parameterId}/currentValue
```

**Purpose**: Devices publish parameter values to these topics  
**Direction**: Device → Alarm System  
**Payload**: Numeric value or JSON object  
**QoS**: AtLeastOnce (QoS 1)

**Example**:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/currentValue
```

#### Alarm Topic (System → Monitoring)
```
{solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}
```

**Purpose**: System publishes active alarms to these topics  
**Direction**: Alarm System → Monitoring/SCADA  
**Payload**: JSON alarm object  
**QoS**: AtLeastOnce (QoS 1)

**Example**:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/alarm/a1234567-89ab-cdef-0123-456789abcdef
```

#### Alarm Acknowledgement Topic (Operator → System)
```
{solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}/ack
```

**Purpose**: Operators publish acknowledgements to clear alarms  
**Direction**: Operator → Alarm System  
**Payload**: JSON acknowledgement object  
**QoS**: AtLeastOnce (QoS 1)

**Example**:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/alarm/a1234567-89ab-cdef-0123-456789abcdef/ack
```

### Wildcard Subscriptions

The system uses MQTT wildcards for efficient subscriptions:

#### Subscribe to All Current Values
```
{solutionId}/+/+/currentValue
```

Example:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/+/+/currentValue
```

#### Subscribe to All Alarms
```
{solutionId}/+/+/alarm/+
```

#### Subscribe to Specific Device
```
{solutionId}/{deviceId}/+/currentValue
```

#### Subscribe to Specific Parameter
```
{solutionId}/+/{parameterId}/currentValue
```

## Message Formats

### Current Value Message

Devices can send either **simple values** or **JSON objects**.

#### Simple Value (Plain Text)
```
5.5
```

#### JSON Value (Recommended)
```json
{
  "Value": 5.5,
  "Timestamp": "2024-01-15T10:30:00Z",
  "Unit": "bar"
}
```

**Properties**:
- `Value` (double): The parameter's current value
- `Timestamp` (DateTime): When the value was measured (ISO 8601)
- `Unit` (string, optional): Unit of measurement

### Alarm Message

Published by the system when a rule is triggered.

```json
{
  "AlarmId": "a1234567-89ab-cdef-0123-456789abcdef",
  "RuleId": "r1234567-89ab-cdef-0123-456789abcdef",
  "AlarmName": "High Pressure Alarm",
  "Severity": "Critical",
  "Priority": "High",
  "CurrentValue": 5.5,
  "TriggeredAt": "2024-01-15T10:30:00Z",
  "Message": "Pressure exceeded maximum threshold",
  "RecommendedAction": "Check pressure relief valve and reduce system load",
  "IsActive": true
}
```

**Properties**:
- `AlarmId` (Guid): Unique alarm instance identifier
- `RuleId` (Guid): Rule that triggered this alarm
- `AlarmName` (string): Human-readable alarm name
- `Severity` (string): Critical, High, Medium, Low
- `Priority` (string): High, Medium, Low
- `CurrentValue` (double): Parameter value when alarm triggered
- `TriggeredAt` (DateTime): When alarm was triggered (ISO 8601)
- `Message` (string): Alarm description
- `RecommendedAction` (string): Suggested corrective actions
- `IsActive` (bool): Whether alarm is currently active

### Alarm Acknowledgement Message

Published by operators to acknowledge and clear alarms.

```json
{
  "AlarmId": "a1234567-89ab-cdef-0123-456789abcdef",
  "AcknowledgedAt": "2024-01-15T10:35:00Z",
  "AcknowledgedBy": "John Smith",
  "Comment": "Issue resolved, pressure normalized"
}
```

**Properties**:
- `AlarmId` (Guid): Alarm being acknowledged
- `AcknowledgedAt` (DateTime): When acknowledgement was made (ISO 8601)
- `AcknowledgedBy` (string): Name or ID of acknowledging operator
- `Comment` (string, optional): Additional notes

## Configuration

MQTT configuration is defined in `appsettings.json`:

```json
{
  "MqttConfiguration": {
    "BrokerHost": "localhost",
    "BrokerPort": 1883,
    "SolutionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "TopicPrefix": "",
    "ReconnectDelay": 5000,
    "CleanSession": true
  }
}
```

**Configuration Properties**:
- `BrokerHost` (string): MQTT broker hostname or IP address
- `BrokerPort` (int): MQTT broker port (default: 1883)
- `SolutionId` (Guid): Unique identifier for this solution instance
- `TopicPrefix` (string, optional): Additional topic prefix (usually empty)
- `ReconnectDelay` (int): Milliseconds to wait before reconnection attempts
- `CleanSession` (bool): Whether to start a clean MQTT session

## Implementation Details

### MqttMonitor Class

The `MqttMonitor` class implements both `IMqttMonitor` and `IDeviceParameterValueProvider`:

```csharp
public class MqttMonitor : IMqttMonitor, IDeviceParameterValueProvider
{
    // Subscribes to: {solutionId}/+/+/currentValue
    Task StartMonitoringAsync();
    
    // Disconnects from broker
    Task StopMonitoringAsync();
    
    // Publishes alarm to: {solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}
    Task PublishAlarmAsync(Guid deviceId, Guid parameterId, Guid alarmId, string alarmData);
    
    // Publishes ack to: {solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}/ack
    Task PublishAlarmAcknowledgementAsync(Guid deviceId, Guid parameterId, Guid alarmId, string ackData);
    
    // Returns current value for device parameter
    double? GetCurrentValue(string deviceId, string parameterName);
}
```

### Parameter Value Storage

The `MqttMonitor` maintains an in-memory dictionary of parameter values:

```csharp
private readonly ConcurrentDictionary<string, double> _parameterValues;
```

**Key Format**: `"{deviceId}:{parameterId}"`  
**Value**: Current parameter value (double)

When a current value message is received:
1. Topic is parsed to extract `deviceId` and `parameterId`
2. Payload is deserialized (JSON or plain text)
3. Value is stored/updated in dictionary
4. Rule engine can query values via `GetCurrentValue()`

### Auto-Reconnection

The monitor includes automatic reconnection logic:

```csharp
private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
{
    _isConnected = false;
    _logger.LogWarning("MQTT client disconnected. Reason: {Reason}", args.Reason);

    if (!args.ClientWasConnected)
    {
        await Task.Delay(_config.ReconnectDelay);
        await StartMonitoringAsync();
    }
}
```

If connection is lost, the system waits `ReconnectDelay` milliseconds and attempts to reconnect.

## Integration with Rule Engine

The MQTT monitor integrates with the rule engine to provide real-time parameter values:

### 1. Registration in Dependency Injection

```csharp
// Program.cs
services.Configure<MqttConfiguration>(configuration.GetSection("MqttConfiguration"));
services.AddSingleton<IMqttMonitor, MqttMonitor>();
services.AddSingleton<IDeviceParameterValueProvider>(sp => 
    sp.GetRequiredService<IMqttMonitor>() as IDeviceParameterValueProvider);
```

### 2. Worker Lifecycle

```csharp
public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start MQTT monitoring on worker startup
        await _mqttMonitor.StartMonitoringAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process parameters and evaluate rules
            await monitorService.ProcessParametersAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        
        // Stop MQTT monitoring on shutdown
        await _mqttMonitor.StopMonitoringAsync();
    }
}
```

### 3. Rule Evaluation Flow

1. Device publishes parameter value to `{solutionId}/{deviceId}/{parameterId}/currentValue`
2. `MqttMonitor` receives message and stores value in `_parameterValues`
3. Worker calls `ProcessParametersAsync()` every 10 seconds
4. Rule engine queries current values via `GetCurrentValue(deviceId, parameterId)`
5. Rules are evaluated using current parameter values
6. If rule triggers, alarm is published to `{solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}`

## Testing

### Using the MQTT Test Client

The test client (`DeviceAlarmSystem.MqttTestClient`) allows you to:
- Publish test parameter values
- Subscribe to alarm topics
- Send acknowledgements

See `tests/DeviceAlarmSystem.MqttTestClient/README.md` for detailed usage instructions.

### Manual Testing with Mosquitto

#### Publish a Current Value
```bash
mosquitto_pub -h localhost -t "a1b2c3d4-e5f6-7890-abcd-ef1234567890/device-123/param-456/currentValue" -m "5.5"
```

#### Subscribe to Alarms
```bash
mosquitto_sub -h localhost -t "a1b2c3d4-e5f6-7890-abcd-ef1234567890/+/+/alarm/+"
```

#### Publish Acknowledgement
```bash
mosquitto_pub -h localhost -t "a1b2c3d4-e5f6-7890-abcd-ef1234567890/device-123/param-456/alarm/alarm-789/ack" -m '{"AlarmId":"alarm-789","AcknowledgedAt":"2024-01-15T10:35:00Z","AcknowledgedBy":"Operator","Comment":"Resolved"}'
```

## Best Practices

### For Devices Publishing Values

1. **Use JSON format** for better extensibility
2. **Include timestamps** to track when values were measured
3. **Publish at appropriate intervals** (not too frequently to avoid flooding)
4. **Use QoS 1** (AtLeastOnce) for important values
5. **Handle connection failures** with retry logic

### For Alarm Consumers

1. **Subscribe with wildcards** to receive all relevant alarms
2. **Store alarm history** for auditing and analysis
3. **Implement acknowledgement workflow** for operator visibility
4. **Monitor QoS** to ensure message delivery
5. **Log all alarm events** with timestamps

### For System Administrators

1. **Configure appropriate reconnect delays** (avoid rapid reconnection attempts)
2. **Monitor MQTT broker performance** (message throughput, connection count)
3. **Set up MQTT broker authentication** for production environments
4. **Use TLS/SSL** for secure communication in production
5. **Configure persistent sessions** if needed for critical alarms

## Security Considerations

### Production Deployment

For production environments, consider:

1. **Authentication**: Enable MQTT username/password authentication
2. **Authorization**: Configure topic-based access control
3. **Encryption**: Use TLS/SSL (port 8883)
4. **Network Segmentation**: Isolate MQTT broker on secure network
5. **Audit Logging**: Log all MQTT connections and message activity

### Example Secure Configuration

```json
{
  "MqttConfiguration": {
    "BrokerHost": "mqtt.production.company.com",
    "BrokerPort": 8883,
    "UseTls": true,
    "Username": "alarm-system",
    "Password": "secure-password",
    "SolutionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "ReconnectDelay": 5000,
    "CleanSession": false
  }
}
```

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to MQTT broker  
**Solutions**:
- Verify broker is running: `netstat -an | findstr 1883`
- Check firewall rules
- Verify broker configuration allows connections
- Check credentials if authentication is enabled

### No Messages Received

**Problem**: Not receiving parameter values or alarms  
**Solutions**:
- Verify topic subscription is correct (check wildcards)
- Check message is published to correct topic
- Verify QoS settings
- Check broker logs for routing issues

### Values Not Updating

**Problem**: Parameter values are stale  
**Solutions**:
- Verify devices are publishing regularly
- Check `MqttMonitor` is running and connected
- Verify topic parsing logic (deviceId/parameterId extraction)
- Check for errors in application logs

### Alarms Not Published

**Problem**: Rules trigger but alarms aren't published  
**Solutions**:
- Verify `PublishAlarmAsync()` is being called
- Check MQTT client is connected
- Verify alarm message serialization
- Check for exceptions in application logs

## Future Enhancements

Potential improvements to the MQTT integration:

1. **Retained Messages**: Publish alarms with retain flag for immediate visibility
2. **Last Will and Testament**: Configure LWT for system health monitoring
3. **Message Buffering**: Queue messages during disconnection
4. **Topic Compression**: Use binary protocols for high-frequency updates
5. **Multi-Broker Support**: Connect to multiple MQTT brokers for redundancy
6. **Alarm Escalation**: Publish to different topics based on severity
7. **Historical Queries**: Store parameter history in time-series database
8. **WebSocket Support**: Enable browser-based alarm monitoring

## References

- [MQTTnet Documentation](https://github.com/dotnet/MQTTnet/wiki)
- [MQTT 5.0 Specification](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html)
- [Mosquitto MQTT Broker](https://mosquitto.org/)
- [HiveMQ MQTT Essentials](https://www.hivemq.com/mqtt-essentials/)
