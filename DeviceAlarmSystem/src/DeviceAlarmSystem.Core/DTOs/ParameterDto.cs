using System;
using System.Collections.Generic;

namespace DeviceAlarmSystem.Core.DTOs
{
    public class ParameterDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public string Unit { get; set; } = string.Empty;

        // Associated rules for this parameter
        public List<RuleDto>? Rules { get; set; }
    }
}
