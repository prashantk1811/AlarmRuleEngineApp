using RulesEngine.Models;
using System.Collections.Generic;

namespace DeviceAlarmSystem.RuleEngine.Models
{
    public class RuleWorkflowDefinition
    {
        public string WorkflowName { get; set; } = string.Empty;
        public List<RuleDefinition> Rules { get; set; } = new();
    }

    public class RuleDefinition
    {
        public string RuleName { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string? SuccessEvent { get; set; }
        public string? ErrorMessage { get; set; }
        public int Priority { get; set; }
    }
}
