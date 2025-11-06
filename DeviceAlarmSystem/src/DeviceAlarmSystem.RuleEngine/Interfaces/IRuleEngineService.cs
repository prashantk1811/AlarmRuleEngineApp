using DeviceAlarmSystem.Core.Entities;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.RuleEngine.Interfaces
{
    public interface IRuleEngineService
    {
        Task<bool> EvaluateAsync(Parameter parameter, DeviceAlarmSystem.Core.Entities.Rule rule);
        Task LoadRulesFromDatabaseAsync();
        Task RefreshRulesAsync();
    }
}
