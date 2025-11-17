using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.MqttTestClient
{
    class Program
    {
        private static readonly Guid SolutionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Test Client");
            Console.WriteLine("================");
            Console.WriteLine();
            
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            // Setup message handler
            client.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                Console.WriteLine();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] *** RECEIVED MESSAGE ***");
                Console.WriteLine($"Topic: {topic}");
                Console.WriteLine($"Payload: {payload}");
                Console.WriteLine();
                await Task.CompletedTask;
            };

            // Connect to broker
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId($"TestClient_{Guid.NewGuid()}")
                .WithCleanSession(true)
                .Build();

            try
            {
                await client.ConnectAsync(options);
                Console.WriteLine("Connected to MQTT broker");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                return;
            }

            // Subscribe to all alarm topics
            var alarmTopic = $"{SolutionId}/+/+/alarm/+";
            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(alarmTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
            
            Console.WriteLine($"Subscribed to: {alarmTopic}");
            Console.WriteLine();

            // Main menu loop
            while (true)
            {
                Console.WriteLine("Commands:");
                Console.WriteLine("  1. Publish Current Value");
                Console.WriteLine("  2. Subscribe to Topic");
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
                        await SubscribeToTopic(client);
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

            Console.Write("Send as JSON? (y/n): ");
            var sendAsJson = Console.ReadLine()?.ToLower() == "y";

            var topic = $"{SolutionId}/{deviceId}/{parameterId}/currentValue";
            string payload;

            if (sendAsJson)
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
            Console.WriteLine($"Published to: {topic}");
            Console.WriteLine($"Payload: {payload}");
            
            // Small delay to allow message handler to process if subscribed to same topic
            Console.WriteLine("Waiting for message delivery...");
            await Task.Delay(500);
        }

        static async Task SubscribeToTopic(IMqttClient client)
        {
            Console.Write("Enter topic to subscribe (press Enter for all currentValue topics): ");
            var topic = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(topic))
            {
                // Default to all current value topics
                topic = $"{SolutionId}/+/+/currentValue";
            }
            else
            {
                // Replace {solutionId} placeholder
                topic = topic.Replace("{solutionId}", SolutionId.ToString());
            }

            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            Console.WriteLine($"Subscribed to: {topic}");
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
            Console.WriteLine($"Published acknowledgement to: {topic}");
            Console.WriteLine($"Payload: {payload}");
        }
    }
}
