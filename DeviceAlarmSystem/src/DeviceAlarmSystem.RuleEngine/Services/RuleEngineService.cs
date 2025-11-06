//using DeviceAlarmSystem.Core.Entities;
//using DeviceAlarmSystem.Core.Interfaces;
//using DeviceAlarmSystem.RuleEngine.Interfaces;
//using Microsoft.Extensions.Logging;
//using RulesEngine.Models;
//using RulesEngine;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.Linq;

//namespace DeviceAlarmSystem.RuleEngine.Services
//{
//    public class RuleEngineService : IRuleEngineService
//    {
//        private readonly IRuleRepository _ruleRepository;
//        private readonly ILogger<RuleEngineService> _logger;
//        private RulesEngine.RulesEngine? _rulesEngine;
//        private List<Workflow> _workflows = new();

//        public RuleEngineService(IRuleRepository ruleRepository, ILogger<RuleEngineService> logger)
//        {
//            _ruleRepository = ruleRepository;
//            _logger = logger;
//        }

//        public async Task LoadRulesFromDatabaseAsync()
//        {
//            _logger.LogInformation("Loading rule definitions from database...");

//            var rules = await _ruleRepository.GetAllAsync();
//            _workflows = new List<Workflow>();

//            foreach (var rule in rules)
//            {
//                var workflow = new Workflow
//                {
//                    WorkflowName = rule.WorkflowName ?? rule.Id.ToString(),
//                    Rules = new List<RulesEngine.Models.Rule>
//                    {
//                        new RulesEngine.Models.Rule
//                        {
//                            RuleName = rule.Description ?? $"Rule-{rule.Id}",
//                            Expression = rule.Expression,
//                            SuccessEvent = "AlarmTriggered",
//                            ErrorMessage = "Condition failed",
//                            RuleExpressionType = RuleExpressionType.LambdaExpression
//                        }
//                    }
//                };

//                _workflows.Add(workflow);
//            }

//            _rulesEngine = new RulesEngine.RulesEngine(_workflows.ToArray(), null);
//            _logger.LogInformation("Loaded {count} rule workflows.", _workflows.Count);
//        }

//        public async Task RefreshRulesAsync()
//        {
//            await LoadRulesFromDatabaseAsync();
//        }

//        public async Task<bool> EvaluateAsync(Parameter parameter, DeviceAlarmSystem.Core.Entities.Rule rule)
//        {
//            if (_rulesEngine == null)
//                await LoadRulesFromDatabaseAsync();

//            var input = new { Value = parameter.CurrentValue };

//            var results = await _rulesEngine!.ExecuteAllRulesAsync(rule.WorkflowName ?? rule.Id.ToString(), input);

//            var isTriggered = results.Any(r => r.IsSuccess);

//            if (isTriggered)
//            {
//                _logger.LogInformation("Rule triggered for parameter {param}: {rule}", parameter.Name, rule.Description);
//            }

//            return isTriggered;
//        }
//    }
//}
using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.RuleEngine.Interfaces;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using DeviceAlarmSystem.Core.Entities;
using System.Linq;

namespace DeviceAlarmSystem.RuleEngine.Services
{
    public class RuleEngineService : IRuleEngineService
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly ILogger<RuleEngineService> _logger;
        private RulesEngine.RulesEngine? _rulesEngine;
        private List<Workflow> _workflows = new();

        public RuleEngineService(IRuleRepository ruleRepository, ILogger<RuleEngineService> logger)
        {
            _ruleRepository = ruleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Load all rules from database and map to RulesEngine workflows
        /// </summary>
        public async Task LoadRulesFromDatabaseAsync()
        {
            _logger.LogInformation("Loading rule definitions from database...");

            var rules = await _ruleRepository.GetAllAsync();
            _workflows = new List<Workflow>();

            foreach (var coreRule in rules)
            {
                // Dynamically build expression from Min/Max
                string expression = BuildExpression(coreRule);

                var engineRule = new RulesEngine.Models.Rule
                {
                    RuleName = coreRule.Name,
                    Expression = expression,
                    SuccessEvent = "AlarmTriggered",
                    ErrorMessage = "Condition failed",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                };

                var workflow = new Workflow
                {
                    WorkflowName = coreRule.Id.ToString(),
                    Rules = new List<RulesEngine.Models.Rule> { engineRule }
                };

                _workflows.Add(workflow);
            }

            _rulesEngine = new RulesEngine.RulesEngine(_workflows.ToArray(), null);

            _logger.LogInformation("Loaded {count} rule workflows.", _workflows.Count);
        }

        /// <summary>
        /// Refresh rules at runtime
        /// </summary>
        public async Task RefreshRulesAsync()
        {
            await LoadRulesFromDatabaseAsync();
        }

        /// <summary>
        /// Evaluate a rule for a specific parameter
        /// </summary>
        /// <param name="parameter">Parameter with current value</param>
        /// <param name="coreRule">Core domain Rule</param>
        /// <returns>true if rule triggered</returns>
        public async Task<bool> EvaluateAsync(Parameter parameter, DeviceAlarmSystem.Core.Entities.Rule coreRule)
        {
            if (_rulesEngine == null)
                await LoadRulesFromDatabaseAsync();


            // Prepare input for RulesEngine (original approach)
            // var input = new { input1 = parameter.CurrentValue };
            // var results = await _rulesEngine!.ExecuteAllRulesAsync(coreRule.Id.ToString(), input);

            // Recommended: Use RuleParameter for primitive input
            var inputs = new[] { new RulesEngine.Models.RuleParameter("input1", parameter.CurrentValue) };
            var results = await _rulesEngine!.ExecuteAllRulesAsync(coreRule.Id.ToString(), inputs);

            var isTriggered = results.Any(r => r.IsSuccess);

            if (isTriggered)
            {
                _logger.LogInformation("Rule triggered for parameter {param}: {rule}", parameter.Name, coreRule.Name);
            }

            return isTriggered;
        }

        /// <summary>
        /// Build a lambda expression string based on Min/Max of Core.Rule
        /// </summary>
        private string BuildExpression(DeviceAlarmSystem.Core.Entities.Rule coreRule)
        {
            if (coreRule.Min.HasValue && coreRule.Max.HasValue)
                return $"input1 >= {coreRule.Min.Value} && input1 <= {coreRule.Max.Value}";
            else if (coreRule.Min.HasValue)
                return $"input1 >= {coreRule.Min.Value}";
            else if (coreRule.Max.HasValue)
                return $"input1 <= {coreRule.Max.Value}";
            else
                return "true"; // No thresholds, always true
        }
    }
}