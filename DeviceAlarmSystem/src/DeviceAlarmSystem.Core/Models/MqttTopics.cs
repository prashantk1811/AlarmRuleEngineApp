using System;

namespace DeviceAlarmSystem.Core.Models
{
    /// <summary>
    /// MQTT topic structure helper
    /// </summary>
    public static class MqttTopics
    {
        /// <summary>
        /// Builds subscription topic for parameter current values
        /// Format: {solutionId}/{deviceId}/{parameterId}/currentValue
        /// </summary>
        public static string BuildCurrentValueTopic(Guid solutionId, Guid deviceId, Guid parameterId)
        {
            return $"{solutionId}/{deviceId}/{parameterId}/currentValue";
        }

        /// <summary>
        /// Builds subscription topic for all parameters of a device
        /// Format: {solutionId}/{deviceId}/+/currentValue
        /// </summary>
        public static string BuildDeviceCurrentValueTopic(Guid solutionId, Guid deviceId)
        {
            return $"{solutionId}/{deviceId}/+/currentValue";
        }

        /// <summary>
        /// Builds subscription topic for all devices
        /// Format: {solutionId}/+/+/currentValue
        /// </summary>
        public static string BuildAllCurrentValuesTopic(Guid solutionId)
        {
            return $"{solutionId}/+/+/currentValue";
        }

        /// <summary>
        /// Builds publish topic for alarm
        /// Format: {solutionId}/{deviceId}/{parameterId}/{alarmId}
        /// </summary>
        public static string BuildAlarmTopic(Guid solutionId, Guid deviceId, Guid parameterId, Guid alarmId)
        {
            return $"{solutionId}/{deviceId}/{parameterId}/{alarmId}";
        }

        /// <summary>
        /// Builds publish topic for alarm acknowledgement
        /// Format: {solutionId}/{deviceId}/{parameterId}/{alarmId}/Ack
        /// </summary>
        public static string BuildAlarmAckTopic(Guid solutionId, Guid deviceId, Guid parameterId, Guid alarmId)
        {
            return $"{solutionId}/{deviceId}/{parameterId}/{alarmId}/Ack";
        }

        /// <summary>
        /// Parses current value topic to extract device and parameter IDs
        /// </summary>
        public static (Guid? deviceId, Guid? parameterId) ParseCurrentValueTopic(string topic)
        {
            var parts = topic.Split('/');
            if (parts.Length >= 4)
            {
                if (Guid.TryParse(parts[1], out var deviceId) && Guid.TryParse(parts[2], out var parameterId))
                {
                    return (deviceId, parameterId);
                }
            }
            return (null, null);
        }
    }

    /// <summary>
    /// MQTT message payload for current value updates
    /// </summary>
    public class CurrentValueMessage
    {
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// MQTT message payload for alarm publication
    /// </summary>
    public class AlarmMessage
    {
        public Guid AlarmId { get; set; }
        public Guid RuleId { get; set; }
        public string? AlarmName { get; set; }
        public string? Severity { get; set; }
        public int Priority { get; set; }
        public double CurrentValue { get; set; }
        public DateTime TriggeredAt { get; set; }
        public string? Message { get; set; }
        public string? RecommendedAction { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// MQTT message payload for alarm acknowledgement
    /// </summary>
    public class AlarmAcknowledgementMessage
    {
        public Guid AlarmId { get; set; }
        public DateTime AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string? Comment { get; set; }
    }
}