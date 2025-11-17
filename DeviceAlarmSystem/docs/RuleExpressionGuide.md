# Rule Expression Guide

## Overview
This guide explains how to work with rule expressions in the DeviceAlarmSystem, including how to dynamically extract parameters and validate expressions.

## Expression Syntax
Rule expressions use C# lambda syntax and can reference parameter names from your YAML configuration or database.

### Simple Expression Examples
```csharp
"Current > 12"
"Temperature >= 10 && Temperature <= 50"
"Voltage < 220 || Current > 15"
"Pressure > 100"
```

## Parameter Handling

### Single Parameter
If your YAML defines:
```yaml
rule:
  expression: Current > 12
parameter:
  name: Current
  unit: A
```

Your code should pass:
```csharp
var parameters = new Dictionary<string, object>
{
    { "Current", 15.5 }
};
var input = ruleEngineService.BuildDynamicInput(parameters);
```

### Multiple Parameters
If your expression is: `Current > 12 && Voltage < 220`

You need to provide both:
```csharp
var parameters = new Dictionary<string, object>
{
    { "Current", 15.5 },
    { "Voltage", 215.0 }
};
var input = ruleEngineService.BuildDynamicInput(parameters);
```

## Dynamic Parameter Extraction

### Extract Parameters from Expression
```csharp
var expression = "Current > 12 && Voltage < 220";
var requiredParams = ruleEngineService.ExtractParameterNames(expression);
// Returns: ["Current", "Voltage"]
```

### Validate Parameters Before Evaluation
```csharp
var expression = "Current > 12 && Voltage < 220";
var parameters = new Dictionary<string, object>
{
    { "Current", 15.5 }
    // Missing Voltage!
};

var (isValid, missingParams) = ruleEngineService.ValidateParametersForExpression(expression, parameters);
if (!isValid)
{
    Console.WriteLine($"Missing parameters: {string.Join(", ", missingParams)}");
    // Output: Missing parameters: Voltage
}
```

## Complete Usage Example

```csharp
// 1. Load rule from database
var rule = await ruleRepository.GetByIdAsync(ruleId);

// 2. Extract required parameters from expression
var requiredParams = ruleEngineService.ExtractParameterNames(rule.Expression);

// 3. Collect parameter values (from MQTT, sensors, etc.)
var parameters = new Dictionary<string, object>();
foreach (var paramName in requiredParams)
{
    var paramValue = await GetParameterValueFromSource(paramName);
    parameters[paramName] = paramValue;
}

// 4. Validate all required parameters are provided
var (isValid, missingParams) = ruleEngineService.ValidateParametersForExpression(rule.Expression, parameters);
if (!isValid)
{
    throw new InvalidOperationException($"Missing parameters: {string.Join(", ", missingParams)}");
}

// 5. Build dynamic input object
var input = ruleEngineService.BuildDynamicInput(parameters);

// 6. Evaluate the rule
var results = await rulesEngine.ExecuteAllRulesAsync(rule.Id.ToString(), input);
var isTriggered = results.Any(r => r.IsSuccess);
```

## Database Schema

Your Rule table should have these fields:

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| Id | GUID | Unique identifier | ... |
| Name | string | Rule name | "HighCurrent" |
| Expression | string | C# lambda expression | "Current > 12" |
| Min | double? | Fallback min threshold | 12.0 |
| Max | double? | Fallback max threshold | null |
| ParameterId | GUID | Associated parameter | ... |
| Description | string | Human-readable description | "High current alarm" |

## YAML Configuration

Example YAML with expression:
```yaml
rule:
  name: HighCurrent
  comparisonType: GreaterThan
  expression: Current > 12
parameter:
  id: P-CUR-001
  name: Current
  unit: A
  key: [mib, device.current.value]
```

## Error Handling

### Missing Parameter at Runtime
If a parameter is missing when evaluating:
```csharp
try
{
    var results = await rulesEngine.ExecuteAllRulesAsync(ruleId, input);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to evaluate rule {RuleId}. Check if all parameters are provided.", ruleId);
}
```

### Invalid Expression Syntax
To validate expression syntax before saving:
```csharp
try
{
    var testParams = ruleEngineService.ExtractParameterNames(expression);
    var testInput = new Dictionary<string, object>();
    foreach (var param in testParams)
    {
        testInput[param] = 0; // Dummy value
    }
    
    // Try to create and compile the rule
    var testRule = new RulesEngine.Models.Rule
    {
        RuleName = "ValidationTest",
        Expression = expression,
        RuleExpressionType = RuleExpressionType.LambdaExpression
    };
    var testWorkflow = new Workflow
    {
        WorkflowName = "ValidationWorkflow",
        Rules = new List<RulesEngine.Models.Rule> { testRule }
    };
    var testEngine = new RulesEngine.RulesEngine(new[] { testWorkflow }, null);
    
    // If we get here, expression syntax is valid
    return true;
}
catch
{
    return false;
}
```

## Advanced Expression Examples

### OR and AND Logic

#### Simple OR Rule
```csharp
// Rule: Trigger if Pressure > 3 OR Temperature > 70
Expression = "Pressure > 3 || Temperature > 70"
```

#### Simple AND Rule
```csharp
// Rule: Trigger if Pressure > 3 AND Temperature > 70
Expression = "Pressure > 3 && Temperature > 70"
```

#### Range Check (AND)
```csharp
// Rule: Trigger if Temperature is between 10 and 50
Expression = "Temperature >= 10 && Temperature <= 50"
```

#### Multiple OR Conditions
```csharp
// Rule: Trigger if any parameter is out of range
Expression = "Pressure > 3 || Temperature > 70 || Current > 12"
```

#### Combined AND/OR
```csharp
// Rule: High pressure AND (high temp OR high current)
Expression = "Pressure > 3 && (Temperature > 70 || Current > 12)"
```

#### Complex Business Logic
```csharp
// Rule: Critical alarm - multiple conditions
Expression = "(Pressure > 5 || Temperature > 90) && Vibration < 2 && Speed > 1000"
```

### Mathematical Operations

#### Power Calculations
```csharp
// Rule: Power consumption = Voltage * Current
Expression = "Voltage * Current > 1000"

// Rule: Area calculation (radius squared)
Expression = "3.14159 * Radius * Radius > 100"

// Rule: Exponential check
Expression = "Math.Pow(Temperature, 2) > 5000"
```

#### Square Root
```csharp
// Rule: RMS (Root Mean Square) calculation
Expression = "Math.Sqrt(Voltage * Voltage + Current * Current) > 500"

// Rule: Distance calculation
Expression = "Math.Sqrt(Math.Pow(X - 10, 2) + Math.Pow(Y - 20, 2)) > 50"
```

#### Absolute Value
```csharp
// Rule: Deviation from setpoint
Expression = "Math.Abs(Temperature - 70) > 5"

// Rule: Pressure differential
Expression = "Math.Abs(InletPressure - OutletPressure) > 2"
```

#### Min/Max Functions
```csharp
// Rule: Any value exceeds threshold
Expression = "Math.Max(Pressure1, Pressure2) > 5"

// Rule: Minimum safety check
Expression = "Math.Min(Temperature1, Temperature2) < 10"

// Rule: Multiple sensors maximum
Expression = "Math.Max(Math.Max(Sensor1, Sensor2), Sensor3) > 100"
```

### Averaging and Statistical Functions

#### Simple Average
```csharp
// Rule: Average of three sensors
Expression = "(Sensor1 + Sensor2 + Sensor3) / 3 > 50"

// Rule: Average temperature check
Expression = "(TempInlet + TempOutlet) / 2 > 75"
```

#### Weighted Average
```csharp
// Rule: Weighted average (70% current, 30% previous)
Expression = "(CurrentValue * 0.7 + PreviousValue * 0.3) > 100"
```

#### Variance/Deviation
```csharp
// Rule: Check variance between sensors
Expression = "Math.Abs(Sensor1 - Sensor2) > 5 || Math.Abs(Sensor2 - Sensor3) > 5"

// Rule: Standard deviation approximation
Expression = "Math.Pow(Value1 - Average, 2) + Math.Pow(Value2 - Average, 2) > 100"
```

### Trigonometric Functions

```csharp
// Rule: Sine wave amplitude check
Expression = "Math.Sin(Phase * 3.14159 / 180) * Amplitude > 50"

// Rule: Circular motion
Expression = "Math.Sqrt(Math.Pow(Math.Cos(Angle), 2) + Math.Pow(Math.Sin(Angle), 2)) > 1.5"

// Rule: Slope calculation
Expression = "Math.Tan(Angle * 3.14159 / 180) > 1.5"
```

### Logarithmic Functions

```csharp
// Rule: Exponential decay
Expression = "Math.Log(Concentration) < -2"

// Rule: pH level calculation
Expression = "-Math.Log10(HydrogenIon) > 7"

// Rule: Exponential growth
Expression = "Math.Exp(GrowthRate * Time) > 1000"

// Rule: Temperature coefficient
Expression = "Math.Exp((Temperature - 25) / 10) > 2"
```

### Rounding and Ceiling/Floor

```csharp
// Rule: Rounded value check
Expression = "Math.Round(Temperature, 0) > 70"

// Rule: Ceiling check
Expression = "Math.Ceiling(Pressure) > 5"

// Rule: Floor check
Expression = "Math.Floor(Flow) < 20"
```

### Percentage Calculations

```csharp
// Rule: Percentage change
Expression = "((CurrentValue - PreviousValue) / PreviousValue) * 100 > 10"

// Rule: Efficiency calculation
Expression = "(OutputPower / InputPower) * 100 < 80"

// Rule: Capacity utilization
Expression = "(CurrentLoad / MaxLoad) * 100 > 90"
```

### Complex Composite Rules

#### Power Consumption with Efficiency
```csharp
// Rule: Inefficient operation detection
Expression = "(Voltage * Current) / OutputPower > 1.2"
```

#### Vibration Analysis
```csharp
// Rule: Overall vibration level (RMS of 3 axes)
Expression = "Math.Sqrt((VibrationX * VibrationX + VibrationY * VibrationY + VibrationZ * VibrationZ) / 3) > 2"
```

#### Temperature Gradient
```csharp
// Rule: Temperature rate of change
Expression = "(CurrentTemp - PreviousTemp) / TimeInterval > 5"

// Rule: Thermal shock detection
Expression = "Math.Abs((CurrentTemp - PreviousTemp) / TimeInterval) > 10"
```

#### Pressure Ratio
```csharp
// Rule: Compression ratio
Expression = "OutletPressure / InletPressure > 4"

// Rule: Pressure drop percentage
Expression = "((InletPressure - OutletPressure) / InletPressure) * 100 > 15"
```

#### Flow Rate Calculation
```csharp
// Rule: Volumetric flow rate
Expression = "Velocity * Area * 3600 > 5000"

// Rule: Mass flow rate
Expression = "Density * Velocity * Area > 1000"
```

#### Energy Calculations
```csharp
// Rule: Kinetic energy
Expression = "0.5 * Mass * Math.Pow(Velocity, 2) > 10000"

// Rule: Potential energy
Expression = "Mass * 9.81 * Height > 5000"

// Rule: Total energy
Expression = "(0.5 * Mass * Math.Pow(Velocity, 2)) + (Mass * 9.81 * Height) > 15000"
```

## Database Examples with Complex Rules

### Example 1: OR Logic Rule
```sql
INSERT INTO "Rules" VALUES
(
    'new-guid-1',
    'parameter-id',
    'PumpCriticalAlarm',
    'Pressure > 3 || Temperature > 70',
    'Critical',
    1,
    'Critical pump condition',
    'Shut down immediately',
    'OR',
    NULL,
    NULL,
    'CriticalCondition'
);
```

### Example 2: Complex AND/OR Rule
```sql
INSERT INTO "Rules" VALUES
(
    'new-guid-2',
    'parameter-id',
    'MotorAbnormal',
    '(Current > 12 || Vibration > 1) && Temperature < 100',
    'High',
    2,
    'Motor operating abnormally',
    'Inspect motor immediately',
    'Complex',
    NULL,
    NULL,
    'AbnormalOperation'
);
```

### Example 3: Power Calculation Rule
```sql
INSERT INTO "Rules" VALUES
(
    'math-rule-001',
    'parameter-id',
    'PowerConsumption',
    'Voltage * Current > 1000',
    'High',
    2,
    'High power consumption',
    'Check for overload',
    'Math',
    NULL,
    NULL,
    'HighPower'
);
```

### Example 4: RMS Vibration Rule
```sql
INSERT INTO "Rules" VALUES
(
    'math-rule-002',
    'parameter-id',
    'VibrationRMS',
    'Math.Sqrt((VibrationX * VibrationX + VibrationY * VibrationY + VibrationZ * VibrationZ) / 3) > 2',
    'Critical',
    1,
    'High vibration detected',
    'Stop machine immediately',
    'RMS',
    NULL,
    NULL,
    'HighVibration'
);
```

### Example 5: Efficiency Rule
```sql
INSERT INTO "Rules" VALUES
(
    'math-rule-003',
    'parameter-id',
    'MotorEfficiency',
    '(OutputPower / InputPower) * 100 < 80',
    'Medium',
    2,
    'Low motor efficiency',
    'Inspect motor performance',
    'Efficiency',
    NULL,
    NULL,
    'LowEfficiency'
);
```

## YAML Examples with Complex Rules

### Example 1: OR Rule in YAML
```yaml
- id: R-CRIT-001
  name: CriticalPumpAlarm
  description: Critical pump condition
  severity: Critical
  priority: 1
  recommendedAction: Shut down pump
  message: Critical alarm triggered
  rule:
    name: CriticalCondition
    comparisonType: OR
    expression: Pressure > 5 || Temperature > 90
  parameter:
    id: P-MULT-001
    name: Multiple
    unit: mixed
    key: [mib, device.status]
```

### Example 2: Complex AND/OR in YAML
```yaml
- id: R-COMP-001
  name: ComplexMotorCheck
  description: Motor operating outside safe parameters
  severity: High
  priority: 1
  recommendedAction: Stop motor and inspect
  message: Motor condition critical
  rule:
    name: ComplexCheck
    comparisonType: Complex
    expression: (Current > 12 && Temperature > 80) || (Vibration > 1.5 && Speed < 1000)
```

### Example 3: Power Calculation in YAML
```yaml
- id: R-MATH-001
  name: PowerOverload
  description: Power consumption exceeds limit
  severity: High
  priority: 1
  recommendedAction: Reduce load
  message: High power detected
  rule:
    name: PowerCheck
    comparisonType: Math
    expression: Voltage * Current > 1000
```

### Example 4: RMS Vibration in YAML
```yaml
- id: R-MATH-002
  name: HighVibration
  description: Vibration RMS exceeds threshold
  severity: Critical
  priority: 1
  recommendedAction: Stop equipment
  message: Dangerous vibration level
  rule:
    name: VibrationRMS
    comparisonType: RMS
    expression: Math.Sqrt((VibrationX * VibrationX + VibrationY * VibrationY + VibrationZ * VibrationZ) / 3) > 2
```

## Available Math Functions

RulesEngine supports standard C# Math class methods:

| Function | Description | Example |
|----------|-------------|---------|
| `Math.Pow(x, y)` | Power/Exponentiation | `Math.Pow(Temperature, 2) > 5000` |
| `Math.Sqrt(x)` | Square root | `Math.Sqrt(Voltage * Voltage + Current * Current) > 500` |
| `Math.Abs(x)` | Absolute value | `Math.Abs(Temperature - 70) > 5` |
| `Math.Min(x, y)` | Minimum | `Math.Min(Temperature1, Temperature2) < 10` |
| `Math.Max(x, y)` | Maximum | `Math.Max(Pressure1, Pressure2) > 5` |
| `Math.Round(x)` | Rounding | `Math.Round(Temperature, 0) > 70` |
| `Math.Ceiling(x)` | Ceiling | `Math.Ceiling(Pressure) > 5` |
| `Math.Floor(x)` | Floor | `Math.Floor(Flow) < 20` |
| `Math.Sin(x)` | Sine | `Math.Sin(Phase * 3.14159 / 180) * Amplitude > 50` |
| `Math.Cos(x)` | Cosine | `Math.Cos(Angle) > 0.5` |
| `Math.Tan(x)` | Tangent | `Math.Tan(Angle * 3.14159 / 180) > 1.5` |
| `Math.Log(x)` | Natural logarithm | `Math.Log(Concentration) < -2` |
| `Math.Log10(x)` | Base-10 logarithm | `-Math.Log10(HydrogenIon) > 7` |
| `Math.Exp(x)` | Exponential | `Math.Exp(GrowthRate * Time) > 1000` |

## Operator Reference

| Operator | Description | Example |
|----------|-------------|---------|
| `&&` | Logical AND | `Pressure > 3 && Temperature > 70` |
| `||` | Logical OR | `Pressure > 3 || Temperature > 70` |
| `!` | Logical NOT | `!(Temperature > 100)` |
| `>` | Greater than | `Temperature > 70` |
| `<` | Less than | `Pressure < 5` |
| `>=` | Greater than or equal | `Temperature >= 70` |
| `<=` | Less than or equal | `Pressure <= 5` |
| `==` | Equal to | `Status == 1` |
| `!=` | Not equal to | `Status != 0` |
| `+` | Addition | `Temperature + Offset > 100` |
| `-` | Subtraction | `InletPressure - OutletPressure > 2` |
| `*` | Multiplication | `Voltage * Current > 1000` |
| `/` | Division | `OutputPower / InputPower < 0.8` |
| `()` | Grouping | `(Pressure > 3 && Temperature > 70) || Current > 12` |

## Best Practices

1. **Always validate expressions before saving to database**
2. **Extract and log required parameters for debugging**
3. **Provide clear error messages when parameters are missing**
4. **Use meaningful parameter names that match your YAML configuration**
5. **Consider using a whitelist of allowed parameter names for security**
6. **Test expressions with sample data before deploying**
7. **For complex math operations, verify performance impact**
8. **Use parentheses to make operator precedence explicit**
9. **Document complex formulas with comments in database/YAML**
10. **Consider multi-parameter requirements when designing rules**
