using System;

namespace DeviceAlarmSystem.Core.Entities
{
    // [ALARM-LOGIC-2025] Alarm state enum
    public enum AlarmState
    {
        ACTIVE,
        ACK,
        RTN,
        ACKRTN
    }

    public enum Severity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Alarm
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Description { get; set; }
        public Severity Severity { get; set; }
        public int Priority { get; set; }
        public required string RecommendedAction { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public Guid RuleId { get; set; }
        public required Rule Rule { get; set; }

        // Optional reference to Parameter
        public Guid ParameterId { get; set; }
        public required Parameter Parameter { get; set; }
        public double CurrentValue { get; set; }       // value that triggered the alarm
        public bool IsActive { get; set; } = true;    // active/inactive state
        public required string Message { get; set; }           // optional message

        // Optional reference to Device
        public Guid DeviceId { get; set; }
        public required Device Device { get; set; }

        // [ALARM-LOGIC-2025] Use enum for alarm state
        public AlarmState State { get; set; } = AlarmState.ACTIVE;
    }
}
