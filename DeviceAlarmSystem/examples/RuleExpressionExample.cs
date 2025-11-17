using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceAlarmSystem.Catalog.AlarmAspect;
using DeviceAlarmSystem.Core.Entities;

namespace DeviceAlarmSystem.Examples
{
    /// <summary>
    /// Example demonstrating how to use the updated rule engine with YAML expressions
    /// </summary>
    public class RuleExpressionExample
    {
        public static async Task Main()
        {
            Console.WriteLine("=== Rule Expression Example ===\n");

            // Example 1: Load YAML aspect with expression
            Console.WriteLine("1. Loading YAML Aspect:");
            var aspectReader = new AlarmAspectReader("FeedPump~1.0", typeof(DiagnosticAlarmAspect));
            var aspect = aspectReader.LoadDiagnosticAlarmAspect("FeedPump~1.0");

            if (aspect != null)
            {
                Console.WriteLine($"   Device: {aspect.DeviceProfile.Name}");
                foreach (var profile in aspect.DeviceProfile.DiagnosticProfile)
                {
                    foreach (var resource in profile.DiagnosticResources)
                    {
                        Console.WriteLine($"   Alarm: {resource.Name}");
                        Console.WriteLine($"   Expression: {resource.Rule?.Expression ?? "N/A"}");
                        Console.WriteLine($"   Parameter: {resource.Parameter.Name} ({resource.Parameter.Unit})");
                        Console.WriteLine();
                    }
                }
            }

            // Example 2: Database Rule with Expression matching parameter name
            Console.WriteLine("\n2. Database Rule Examples:");
            Console.WriteLine("   Expression: 'Pressure > 3'");
            Console.WriteLine("   Parameter Name: 'Pressure'");
            Console.WriteLine("   Parameter Value: 3.5");
            Console.WriteLine("   Result: Will be evaluated with { Pressure: 3.5 }");

            Console.WriteLine("\n   Expression: 'Temperature > 70'");
            Console.WriteLine("   Parameter Name: 'Temperature'");
            Console.WriteLine("   Parameter Value: 75.0");
            Console.WriteLine("   Result: Will be evaluated with { Temperature: 75.0 }");

            // Example 3: Using ExtractParameterNames
            Console.WriteLine("\n3. Parameter Extraction:");
            var expressions = new[]
            {
                "Pressure > 3",
                "Current > 12 && Voltage < 220",
                "input1 >= 10 && input1 <= 50"
            };

            // Note: In actual usage, you would get RuleEngineService from DI
            // This is just for demonstration
            foreach (var expr in expressions)
            {
                Console.WriteLine($"   Expression: {expr}");
                // var params = ruleEngineService.ExtractParameterNames(expr);
                // Console.WriteLine($"   Parameters: {string.Join(", ", params)}");
            }

            // Example 4: Multi-parameter expression
            Console.WriteLine("\n4. Multi-Parameter Expression:");
            Console.WriteLine("   YAML: Two separate resources with different parameters");
            Console.WriteLine("   Database: Single expression like 'Pressure > 3 && Temperature < 70'");
            Console.WriteLine("   Required Input: { Pressure: 3.5, Temperature: 65.0 }");

            // Example 5: Migration from input1 to parameter names
            Console.WriteLine("\n5. Migration Path:");
            Console.WriteLine("   Old: Expression='input1 > 3', Input={ input1: 3.5 }");
            Console.WriteLine("   New: Expression='Pressure > 3', Input={ Pressure: 3.5 }");
            Console.WriteLine("   System handles both automatically!");

            Console.WriteLine("\n=== Example Complete ===");
        }
    }
}
