using System;

namespace DeviceAlarmSystem.Core.Models
{
    public class MqttConfiguration
    {
        public string BrokerHost { get; set; } = "localhost";
        public int BrokerPort { get; set; } = 1883;
        public Guid SolutionId { get; set; }
        public string TopicPrefix { get; set; } = "devicealarm";
        public int ReconnectDelay { get; set; } = 5000;
        public bool CleanSession { get; set; } = true;
    }
}