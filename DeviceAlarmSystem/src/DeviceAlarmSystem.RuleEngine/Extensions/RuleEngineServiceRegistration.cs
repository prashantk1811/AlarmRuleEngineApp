using DeviceAlarmSystem.RuleEngine.Interfaces;
using DeviceAlarmSystem.RuleEngine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceAlarmSystem.RuleEngine.Extensions
{
    public static class RuleEngineServiceRegistration
    {
        public static IServiceCollection AddRuleEngine(this IServiceCollection services)
        {
            services.AddScoped<IRuleEngineService, RuleEngineService>();
            services.AddScoped<RuleEvaluator>();
            return services;
        }
    }
}
