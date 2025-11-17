using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.MqttTestClient
{
    class Publisher
    {
        private static readonly Guid SolutionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Publisher - Device Alarm System");
            Console.WriteLine("====================================");
            Console.WriteLine($"Solution ID: {SolutionId}");
            Console.WriteLine();
            
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            // Connect to broker
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId($"Publisher_{Guid.NewGuid()}")
                .WithCleanSession(true)
                .Build();

            try
            {
                await client.ConnectAsync(options);
                Console.WriteLine("✓ Connected to MQTT broker at localhost:1883");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to connect: {ex.Message}");
                return;
            }

            // Main menu loop
            while (true)
            {
                Console.WriteLine("Publisher Commands:");
                Console.WriteLine("  1. Publish Current Value");
                Console.WriteLine("  2. Publish Multiple Values (continuous)");
                Console.WriteLine("  3. Publish Alarm Acknowledgement");
                Console.WriteLine("  4. Exit");
                Console.Write("Select command: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await PublishCurrentValue(client);
                        break;
                    case "2":
                        await PublishContinuousValues(client);
                        break;
                    case "3":
                        await PublishAlarmAck(client);
                        break;
                    case "4":
                        Console.WriteLine("Disconnecting...");
                        await client.DisconnectAsync();
                        return;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }

                Console.WriteLine();
            }
        }

        static async Task PublishCurrentValue(IMqttClient client)
        {
            Console.Write("Enter Device ID (GUID or press Enter for sample): ");
            var deviceIdInput = Console.ReadLine();
            var deviceId = string.IsNullOrWhiteSpace(deviceIdInput) 
                ? Guid.NewGuid() 
                : Guid.Parse(deviceIdInput);

            Console.Write("Enter Parameter ID (GUID or press Enter for sample): ");
            var paramIdInput = Console.ReadLine();
            var parameterId = string.IsNullOrWhiteSpace(paramIdInput) 
                ? Guid.NewGuid() 
                : Guid.Parse(paramIdInput);

            Console.Write("Enter current value (e.g., 5.5): ");
            if (!double.TryParse(Console.ReadLine(), out var value))
            {
                Console.WriteLine("Invalid value");
                return;
            }

            Console.Write("Send as JSON? (y/n, default=y): ");
            var jsonInput = Console.ReadLine()?.ToLower();
            var sendAsJson = string.IsNullOrWhiteSpace(jsonInput) || jsonInput == "y";

            await PublishValue(client, deviceId, parameterId, value, sendAsJson);
        }

        static async Task PublishContinuousValues(IMqttClient client)
        {
            Console.Write("Enter Device ID (GUID or press Enter for sample): ");
            var deviceIdInput = Console.ReadLine();
            var deviceId = string.IsNullOrWhiteSpace(deviceIdInput) 
                ? Guid.NewGuid() 
                : Guid.Parse(deviceIdInput);

            Console.Write("Enter Parameter ID (GUID or press Enter for sample): ");
            var paramIdInput = Console.ReadLine();
            var parameterId = string.IsNullOrWhiteSpace(paramIdInput) 
                ? Guid.NewGuid() 
                : Guid.Parse(paramIdInput);

            Console.Write("Enter starting value: ");
            if (!double.TryParse(Console.ReadLine(), out var startValue))
            {
                Console.WriteLine("Invalid value");
                return;
            }

            Console.Write("Enter increment per message (e.g., 0.5): ");
            if (!double.TryParse(Console.ReadLine(), out var increment))
            {
                Console.WriteLine("Invalid increment");
                return;
            }

            Console.Write("Enter interval in milliseconds (default=2000): ");
            var intervalInput = Console.ReadLine();
            var interval = string.IsNullOrWhiteSpace(intervalInput) ? 2000 : int.Parse(intervalInput);

            Console.WriteLine();
            Console.WriteLine("Publishing continuous values... Press any key to stop.");
            Console.WriteLine($"Device: {deviceId}");
            Console.WriteLine($"Parameter: {parameterId}");
            Console.WriteLine();

            var currentValue = startValue;
            var random = new Random();
            
            while (!Console.KeyAvailable)
            {
                // Add some random variation
                var variation = (random.NextDouble() - 0.5) * 0.2;
                currentValue += increment + variation;
                
                await PublishValue(client, deviceId, parameterId, currentValue, true);
                await Task.Delay(interval);
            }

            Console.ReadKey(true); // Consume the key press
            Console.WriteLine("Stopped continuous publishing");
        }

        static async Task PublishValue(IMqttClient client, Guid deviceId, Guid parameterId, double value, bool asJson)
        {
            var topic = $"{SolutionId}/{deviceId}/{parameterId}/currentValue";
            string payload;

            if (asJson)
            {
                var message = new
                {
                    Value = value,
                    Timestamp = DateTime.UtcNow,
                    Unit = "bar"
                };
                payload = JsonSerializer.Serialize(message);
            }
            else
            {
                payload = value.ToString();
            }

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.PublishAsync(mqttMessage);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Published: {value:F2} → {topic}");
        }

        static async Task PublishAlarmAck(IMqttClient client)
        {
            Console.Write("Enter Device ID (GUID): ");
            if (!Guid.TryParse(Console.ReadLine(), out var deviceId))
            {
                Console.WriteLine("Invalid Device ID");
                return;
            }

            Console.Write("Enter Parameter ID (GUID): ");
            if (!Guid.TryParse(Console.ReadLine(), out var parameterId))
            {
                Console.WriteLine("Invalid Parameter ID");
                return;
            }

            Console.Write("Enter Alarm ID (GUID): ");
            if (!Guid.TryParse(Console.ReadLine(), out var alarmId))
            {
                Console.WriteLine("Invalid Alarm ID");
                return;
            }

            Console.Write("Enter your name: ");
            var acknowledgedBy = Console.ReadLine();

            Console.Write("Enter comment (optional): ");
            var comment = Console.ReadLine();

            var ackMessage = new
            {
                AlarmId = alarmId,
                AcknowledgedAt = DateTime.UtcNow,
                AcknowledgedBy = acknowledgedBy,
                Comment = comment
            };

            var topic = $"{SolutionId}/{deviceId}/{parameterId}/alarm/{alarmId}/ack";
            var payload = JsonSerializer.Serialize(ackMessage);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.PublishAsync(mqttMessage);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Published acknowledgement to: {topic}");
            Console.WriteLine($"Payload: {payload}");
        }
    }
}
