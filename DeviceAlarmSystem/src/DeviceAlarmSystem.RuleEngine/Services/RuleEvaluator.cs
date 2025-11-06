using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.RuleEngine.Interfaces;
using System;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.RuleEngine.Services
{
    public class RuleEvaluator
    {
        private readonly IRuleEngineService _ruleEngineService;

        public RuleEvaluator(IRuleEngineService ruleEngineService)
        {
            _ruleEngineService = ruleEngineService;
        }

        public async Task<Alarm?> EvaluateAndGenerateAlarmAsync(Parameter parameter, Rule rule)
        {
            // [ALARM-LOGIC-2025] Generate alarm with state ACTIVE if triggered
            bool triggered = await _ruleEngineService.EvaluateAsync(parameter, rule);

            if (triggered)
            {
                return new Alarm
                {
                    Id = Guid.NewGuid(),
                    ParameterId = parameter.Id,
                    RuleId = rule.Id,
                    TriggeredAt = DateTime.UtcNow,
                    CurrentValue = parameter.CurrentValue,
                    IsActive = true,
                    State = AlarmState.ACTIVE, // [ALARM-LOGIC-2025]
                    Message = $"{parameter.Name}: {rule.Description ?? "Rule triggered"} triggered at {parameter.CurrentValue}",
                    Description = rule.Description ?? string.Empty,
                    RecommendedAction = rule.RecommendedAction ?? string.Empty,
                    Rule = rule,
                    Parameter = parameter,
                    Device = parameter.Device ?? new Device { Id = parameter.DeviceId, Name = "Unknown" },
                    DeviceId = parameter.DeviceId
                };
            }

            return null;
        }
    }
}
