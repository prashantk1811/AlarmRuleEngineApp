using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.MqttTestClient
{
    class Subscriber
    {
        private static readonly Guid SolutionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        private static int _messageCount = 0;
        private static readonly Dictionary<string, int> _topicCounts = new Dictionary<string, int>();
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Subscriber - Device Alarm System");
            Console.WriteLine("=====================================");
            Console.WriteLine($"Solution ID: {SolutionId}");
            Console.WriteLine();
            
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            // Setup message handler
            client.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                
                _messageCount++;
                
                if (!_topicCounts.ContainsKey(topic))
                {
                    _topicCounts[topic] = 0;
                }
                _topicCounts[topic]++;
                
                Console.WriteLine();
                Console.WriteLine($"╔════════════════════════════════════════════════════════════════");
                Console.WriteLine($"║ MESSAGE #{_messageCount} - {DateTime.Now:HH:mm:ss.fff}");
                Console.WriteLine($"╠════════════════════════════════════════════════════════════════");
                Console.WriteLine($"║ Topic: {topic}");
                Console.WriteLine($"║ Count: {_topicCounts[topic]} messages on this topic");
                Console.WriteLine($"╠════════════════════════════════════════════════════════════════");
                
                // Try to parse as JSON for better formatting
                if (TryParseJson(payload, out var jsonFormatted))
                {
                    Console.WriteLine($"║ Payload (JSON):");
                    foreach (var line in jsonFormatted.Split('\n'))
                    {
                        Console.WriteLine($"║   {line}");
                    }
                }
                else
                {
                    Console.WriteLine($"║ Payload: {payload}");
                }
                
                Console.WriteLine($"╚════════════════════════════════════════════════════════════════");
                Console.WriteLine();
                
                await Task.CompletedTask;
            };

            // Connect to broker
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId($"Subscriber_{Guid.NewGuid()}")
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

            // Auto-subscribe to common topics
            await SubscribeToDefaultTopics(client);

            // Main menu loop
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Subscriber Commands:");
                Console.WriteLine("  1. Subscribe to Custom Topic");
                Console.WriteLine("  2. Show Subscription Statistics");
                Console.WriteLine("  3. Clear Statistics");
                Console.WriteLine("  4. Re-subscribe to Default Topics");
                Console.WriteLine("  5. Exit");
                Console.Write("Select command: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await SubscribeToCustomTopic(client);
                        break;
                    case "2":
                        ShowStatistics();
                        break;
                    case "3":
                        ClearStatistics();
                        break;
                    case "4":
                        await SubscribeToDefaultTopics(client);
                        break;
                    case "5":
                        Console.WriteLine("Disconnecting...");
                        await client.DisconnectAsync();
                        return;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        static async Task SubscribeToDefaultTopics(IMqttClient client)
        {
            var topics = new[]
            {
                $"{SolutionId}/+/+/currentValue",
                $"{SolutionId}/+/+/alarm/+",
                $"{SolutionId}/+/+/alarm/+/ack"
            };

            Console.WriteLine("Subscribing to default topics:");
            foreach (var topic in topics)
            {
                await client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());
                Console.WriteLine($"  ✓ {topic}");
            }
            Console.WriteLine();
            Console.WriteLine("Listening for messages... (Press Enter to show menu)");
        }

        static async Task SubscribeToCustomTopic(IMqttClient client)
        {
            Console.Write("Enter topic (use {solutionId} as placeholder): ");
            var topic = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(topic))
            {
                Console.WriteLine("Invalid topic");
                return;
            }

            // Replace placeholder
            topic = topic.Replace("{solutionId}", SolutionId.ToString());

            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            Console.WriteLine($"✓ Subscribed to: {topic}");
            Console.WriteLine("Listening for messages...");
        }

        static void ShowStatistics()
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("SUBSCRIPTION STATISTICS");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"Total Messages Received: {_messageCount}");
            Console.WriteLine($"Unique Topics: {_topicCounts.Count}");
            Console.WriteLine();
            
            if (_topicCounts.Count > 0)
            {
                Console.WriteLine("Messages per Topic:");
                foreach (var kvp in _topicCounts)
                {
                    Console.WriteLine($"  {kvp.Value,4} messages - {kvp.Key}");
                }
            }
            else
            {
                Console.WriteLine("No messages received yet.");
            }
            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }

        static void ClearStatistics()
        {
            _messageCount = 0;
            _topicCounts.Clear();
            Console.WriteLine("✓ Statistics cleared");
        }

        static bool TryParseJson(string payload, out string formatted)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                formatted = JsonSerializer.Serialize(doc, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                return true;
            }
            catch
            {
                formatted = null;
                return false;
            }
        }
    }
}
