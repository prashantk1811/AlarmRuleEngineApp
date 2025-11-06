using System;
using System.Collections.Generic;

namespace DeviceAlarmSystem.Core.Entities
{
    public class Parameter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Unit { get; set; }
        public double CurrentValue { get; set; }

        // Foreign Key
        public Guid DeviceId { get; set; }
    public Device? Device { get; set; }

        // Navigation
    public ICollection<Rule>? Rules { get; set; }
    }
}
