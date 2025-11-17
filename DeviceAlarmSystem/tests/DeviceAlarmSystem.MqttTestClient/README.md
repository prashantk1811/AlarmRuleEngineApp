# MQTT Test Client

This is a console-based MQTT test client for testing the Device Alarm System's MQTT integration.

## Features

- **Publish Current Values**: Send device parameter values to MQTT topics
- **Subscribe to Topics**: Listen to specific MQTT topics with wildcard support
- **Publish Alarm Acknowledgements**: Send alarm acknowledgement messages
- **Auto-subscribe to Alarms**: Automatically subscribes to all alarm topics on startup

## Prerequisites

- MQTT broker running on localhost:1883 (or configure a different broker)
- .NET 8.0 SDK installed

## Running the Test Client

### Option 1: Using dotnet CLI

```bash
cd DeviceAlarmSystem\tests\DeviceAlarmSystem.MqttTestClient
dotnet run
```

### Option 2: From build directory

```bash
cd build\Debug\net8.0
DeviceAlarmSystem.MqttTestClient.exe
```

## MQTT Topic Structure

The test client uses the following topic patterns:

### Current Value Topics (for publishing)
```
{solutionId}/{deviceId}/{parameterId}/currentValue
```

Example:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/device-123/param-456/currentValue
```

### Alarm Topics (for receiving)
```
{solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}
```

Example:
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890/device-123/param-456/alarm/alarm-789
```

### Alarm Acknowledgement Topics (for publishing)
```
{solutionId}/{deviceId}/{parameterId}/alarm/{alarmId}/ack
```

## Usage Examples

### 1. Publishing a Current Value

When prompted:
- Press `1` to select "Publish Current Value"
- Enter Device ID (or press Enter to generate a random GUID)
- Enter Parameter ID (or press Enter to generate a random GUID)
- Enter the current value (e.g., `5.5`)
- Choose whether to send as JSON (y/n)

**Example - Simple Value:**
```
Enter Device ID: d1234567-89ab-cdef-0123-456789abcdef
Enter Parameter ID: p1234567-89ab-cdef-0123-456789abcdef
Enter current value: 5.5
Send as JSON?: n

Published to: a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/currentValue
Payload: 5.5
```

**Example - JSON Value:**
```
Enter Device ID: d1234567-89ab-cdef-0123-456789abcdef
Enter Parameter ID: p1234567-89ab-cdef-0123-456789abcdef
Enter current value: 5.5
Send as JSON?: y

Published to: a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/currentValue
Payload: {"Value":5.5,"Timestamp":"2024-01-15T10:30:00Z","Unit":"bar"}
```

### 2. Subscribing to Custom Topics

Press `2` to subscribe to custom topics with wildcard support:

**Subscribe to all current values:**
```
Enter topic to subscribe: {solutionId}/+/+/currentValue
Subscribed to: a1b2c3d4-e5f6-7890-abcd-ef1234567890/+/+/currentValue
```

**Subscribe to specific device:**
```
Enter topic to subscribe: {solutionId}/d1234567-89ab-cdef-0123-456789abcdef/+/currentValue
Subscribed to: a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/+/currentValue
```

### 3. Publishing Alarm Acknowledgements

Press `3` to acknowledge an alarm:

```
Enter Device ID: d1234567-89ab-cdef-0123-456789abcdef
Enter Parameter ID: p1234567-89ab-cdef-0123-456789abcdef
Enter Alarm ID: a1234567-89ab-cdef-0123-456789abcdef
Enter your name: John Smith
Enter comment: Issue resolved, pressure normalized

Published acknowledgement to: a1b2c3d4-e5f6-7890-abcd-ef1234567890/d1234567-89ab-cdef-0123-456789abcdef/p1234567-89ab-cdef-0123-456789abcdef/alarm/a1234567-89ab-cdef-0123-456789abcdef/ack
Payload: {"AlarmId":"a1234567-89ab-cdef-0123-456789abcdef","AcknowledgedAt":"2024-01-15T10:35:00Z","AcknowledgedBy":"John Smith","Comment":"Issue resolved, pressure normalized"}
```

## Testing with the Worker

1. Start the MQTT Broker (e.g., Mosquitto)
2. Run the Worker application (DeviceAlarmSystem.Worker)
3. Run this test client
4. Use the test client to publish parameter values
5. Watch for alarms being published by the Worker when rules are triggered

## Default Configuration

- **Solution ID**: `a1b2c3d4-e5f6-7890-abcd-ef1234567890` (hardcoded to match Worker configuration)
- **Broker Host**: `localhost`
- **Broker Port**: `1883`
- **QoS Level**: `AtLeastOnce` (QoS 1)

## Troubleshooting

### Connection Failed

If you see "Failed to connect", ensure:
1. MQTT broker is running on localhost:1883
2. No firewall blocking port 1883
3. Broker allows anonymous connections

### No Messages Received

If you're not receiving messages:
1. Check topic subscription is correct
2. Verify message is being published to the correct topic
3. Check MQTT broker logs for routing issues

## MQTT Broker Setup

If you don't have an MQTT broker installed, you can use Mosquitto:

**Windows:**
```bash
# Install using Chocolatey
choco install mosquitto

# Start the service
net start mosquitto
```

**Linux:**
```bash
# Install
sudo apt-get install mosquitto mosquitto-clients

# Start
sudo systemctl start mosquitto
```

**Docker:**
```bash
docker run -d -p 1883:1883 -p 9001:9001 eclipse-mosquitto
```
