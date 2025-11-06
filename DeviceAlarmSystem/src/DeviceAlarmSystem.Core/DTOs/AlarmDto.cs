using System;

namespace DeviceAlarmSystem.Core.DTOs
{
    public class AlarmDto
    {
        public Guid Id { get; set; }
        public Guid RuleId { get; set; }
        public Guid ParameterId { get; set; }
        public string Message { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public DateTime TriggeredAt { get; set; }
        public bool IsActive { get; set; }
        public string Severity { get; set; } = "Medium";
    }
}
