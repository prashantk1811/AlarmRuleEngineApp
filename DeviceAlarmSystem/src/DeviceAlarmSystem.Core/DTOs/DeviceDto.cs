using System;
using System.Collections.Generic;

namespace DeviceAlarmSystem.Core.DTOs
{
    public class DeviceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // A device can have multiple parameters
        public List<ParameterDto>? Parameters { get; set; }
    }
}
