using System;
using System.Collections.Generic;

namespace DeviceAlarmSystem.Core.Entities
{
    public class Rule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }

        // Rule criteria
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? ComparisonType { get; set; } // e.g., "GreaterThan", "LessThan", "Range"

        /// <summary>
        /// Custom rule expression (e.g., "input1 > 10 && input2 < 5")
        /// </summary>
        public string? Expression { get; set; }

        // Foreign Key
        public Guid ParameterId { get; set; }
    public Parameter? Parameter { get; set; }

        // Navigation
    public ICollection<Alarm>? Alarms { get; set; }

    public string? Description { get; set; }          // add
    public string? WorkflowName { get; set; }        // add
        public string? RecommendedAction { get; set; }
    }
}
