using System;

namespace DeviceAlarmSystem.Core.DTOs
{
    public class RuleDto
    {
        public Guid Id { get; set; }
        public Guid ParameterId { get; set; }

        // The expression evaluated by Microsoft RulesEngine
        public string Expression { get; set; } = string.Empty;

        public string Severity { get; set; } = "Medium";
        public int Priority { get; set; } = 1;

        public string Description { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }
}
