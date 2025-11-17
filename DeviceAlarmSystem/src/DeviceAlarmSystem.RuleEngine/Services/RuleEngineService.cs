using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.RuleEngine.Interfaces;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using DeviceAlarmSystem.Core.Entities;
using System.Linq;
using System.Text.RegularExpressions;
using System.Dynamic;

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

            _logger.LogInformation("Loaded {count} rule workflows from database.", _workflows.Count);
            _logger.LogInformation("Rule IDs added: {ruleIds}", string.Join(", ", _workflows.Select(w => w.WorkflowName)));
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

            // Build expression
            string expression = BuildExpression(coreRule);

            // Extract parameter names from expression
            var requiredParams = ExtractParameterNames(expression);

            // Build parameters dictionary
            var parameters = new Dictionary<string, object>();
            
            // If expression uses parameter name, use it; otherwise default to input1
            if (parameter.Name != null && requiredParams.Contains(parameter.Name))
            {
                parameters[parameter.Name] = parameter.CurrentValue;
            }
            else if (requiredParams.Contains("input1"))
            {
                parameters["input1"] = parameter.CurrentValue;
            }
            else
            {
                // Default behavior: assume single parameter as input1
                parameters["input1"] = parameter.CurrentValue;
            }

            // Validate all required parameters are provided
            var (isValid, missingParams) = ValidateParametersForExpression(expression, parameters);
            if (!isValid)
            {
                _logger.LogWarning("Missing parameters for rule {RuleId}: {MissingParams}", 
                    coreRule.Id, string.Join(", ", missingParams));
                return false;
            }

            // Build dynamic input
            var input = BuildDynamicInput(parameters);
            var results = await _rulesEngine!.ExecuteAllRulesAsync(coreRule.Id.ToString(), input);

            var isTriggered = ((IEnumerable<RuleResultTree>)results).Any(r => r.IsSuccess);

            if (isTriggered)
            {
                _logger.LogInformation("Rule triggered for parameter {param}: {rule}", parameter.Name, coreRule.Name);
            }

            return isTriggered;
        }

        /// <summary>
        /// Build a lambda expression string based on Expression or Min/Max of Core.Rule
        /// </summary>
        private string BuildExpression(DeviceAlarmSystem.Core.Entities.Rule coreRule)
        {
            // Use custom expression if provided
            if (!string.IsNullOrWhiteSpace(coreRule.Expression))
                return coreRule.Expression;

            // Fall back to Min/Max logic
            if (coreRule.Min.HasValue && coreRule.Max.HasValue)
                return $"input1 >= {coreRule.Min.Value} && input1 <= {coreRule.Max.Value}";
            else if (coreRule.Min.HasValue)
                return $"input1 >= {coreRule.Min.Value}";
            else if (coreRule.Max.HasValue)
                return $"input1 <= {coreRule.Max.Value}";
            else
                return "true"; // No thresholds, always true
        }

        /// <summary>
        /// Extract parameter names from a rule expression
        /// </summary>
        /// <param name="expression">The rule expression string</param>
        /// <returns>List of parameter names referenced in the expression</returns>
        public List<string> ExtractParameterNames(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return new List<string>();

            // Match C# identifiers (variable names)
            var matches = Regex.Matches(expression, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b");
            
            // C# keywords and common operators to exclude
            var keywords = new HashSet<string> 
            { 
                "true", "false", "null", "and", "or", "not", 
                "new", "var", "int", "double", "string", "bool"
            };
            
            return matches
                .Select(m => m.Value)
                .Where(name => !keywords.Contains(name.ToLower()) && !double.TryParse(name, out _))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Validate that all parameters required by the expression are provided
        /// </summary>
        /// <param name="expression">The rule expression</param>
        /// <param name="parameters">Dictionary of parameter name to value</param>
        /// <returns>Tuple with validation result and list of missing parameters</returns>
        public (bool isValid, List<string> missingParams) ValidateParametersForExpression(
            string expression, 
            Dictionary<string, object> parameters)
        {
            var requiredParams = ExtractParameterNames(expression);
            var missingParams = requiredParams
                .Where(param => !parameters.ContainsKey(param))
                .ToList();
            
            return (missingParams.Count == 0, missingParams);
        }

        /// <summary>
        /// Build dynamic input object from parameter dictionary
        /// </summary>
        /// <param name="parameters">Dictionary of parameter name to value</param>
        /// <returns>Dynamic object with parameters as properties</returns>
        public dynamic BuildDynamicInput(Dictionary<string, object> parameters)
        {
            var expandoObject = new ExpandoObject();
            var dictionary = (IDictionary<string, object?>)expandoObject;
            
            foreach (var kvp in parameters)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
            
            return expandoObject;
        }
    }
}