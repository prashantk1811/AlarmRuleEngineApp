using System;
using System.Collections.Generic;

namespace DeviceAlarmSystem.Core.Entities
{
    public class Device
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Description { get; set; }

        // [ALARM-LOGIC-2025] Add Inhibit flag and DeviceType
        public bool Inhibit { get; set; } = false;
    public string? DeviceType { get; set; }

        // Navigation
    public ICollection<Parameter>? Parameters { get; set; }
    }
}
