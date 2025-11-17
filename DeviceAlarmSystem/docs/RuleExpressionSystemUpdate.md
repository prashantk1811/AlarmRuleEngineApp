# Rule Expression System Update - Summary

## Overview
The system has been updated to support dynamic parameter names in rule expressions, allowing expressions like `Pressure > 3` instead of just `input1 > 3`. This enables better alignment with YAML configuration files and more intuitive rule definitions.

## Changes Made

### 1. RuleEngineService.cs
**Location**: `src/DeviceAlarmSystem.RuleEngine/Services/RuleEngineService.cs`

#### Updated Methods:

**BuildExpression()**
- Now prioritizes `coreRule.Expression` property if set
- Falls back to generating expression from `Min`/`Max` properties
- Supports both custom expressions and legacy threshold-based rules

**EvaluateAsync()**
- Extracts parameter names from expressions using `ExtractParameterNames()`
- Matches parameter values to expression variables by name
- Validates all required parameters are provided before evaluation
- Uses dynamic input objects instead of hardcoded `input1`
- Logs warnings when parameters are missing

#### New Methods:

**ExtractParameterNames(string expression)**
- Parses expressions to find all variable references
- Excludes C# keywords and numeric literals
- Returns list of required parameter names

**ValidateParametersForExpression(string expression, Dictionary<string, object> parameters)**
- Checks if all required parameters are provided
- Returns validation result and list of missing parameters

**BuildDynamicInput(Dictionary<string, object> parameters)**
- Creates dynamic ExpandoObject from parameter dictionary
- Allows RulesEngine to access parameters by name

### 2. Database Schema (DeviceAlarmDB.sqlite.sql)
**Location**: `src/DeviceAlarmSystem.Worker/DB/DeviceAlarmDB.sqlite.sql`

#### Updated Expression Column:
All rule expressions now use parameter names instead of `input1`:

| Old Expression | New Expression | Parameter Name |
|---------------|----------------|----------------|
| `input1 > 3` | `Pressure > 3` | Pressure |
| `input1 > 70` | `Temperature > 70` | Temperature |
| `input1 > 12` | `Current > 12` | Current |
| `input1 > 1` | `Vibration > 1` | Vibration |
| `input1 < 90` | `Position < 90` | Position |
| `input1 < 6` | `Pressure < 6` | Pressure |
| `input1 > 130` | `Temperature > 130` | Temperature |
| `input1 < 1400` | `Speed < 1400` | Speed |
| ... | ... | ... |

**Benefits:**
- Expressions are self-documenting
- Match parameter names in YAML files
- Easier to understand and maintain
- Support for multi-parameter expressions

### 3. AlarmAspectReader.cs
**Location**: `src/DeviceAlarmSystem.Catalog.AlarmAspect/src/AlarmAspects/AlarmAspectReader.cs`

#### Updated:
- Made `fileAspect` a class field for reuse
- Added `LoadDiagnosticAlarmAspect()` method to load aspects from YAML
- Added `HasAspect()` method to check aspect existence
- Better encapsulation and separation of concerns

#### New Methods:

**LoadDiagnosticAlarmAspect(string aspectName)**
- Loads a specific aspect from YAML by name
- Returns `DiagnosticAlarmAspect` object with all alarm definitions
- Expressions from YAML are available in `resource.Rule.Expression`

**HasAspect(string aspectName, string aspectTypeName)**
- Checks if an aspect exists in the catalog
- Useful for validation before loading

### 4. YAML Structure
**Location**: `src/DeviceAlarmSystem.Catalog.AlarmAspect/Content/*.yaml`

Existing YAML files already have the correct structure:
```yaml
rule:
  name: HighPressure
  comparisonType: GreaterThan
  expression: Pressure > 3  # Uses parameter name
parameter:
  name: Pressure  # Matches expression variable
  unit: bar
```

## Usage Examples

### Example 1: Simple Rule with Parameter Name
```csharp
var rule = new Rule
{
    Expression = "Pressure > 3",
    ParameterId = pressureParamId
};

var parameter = new Parameter
{
    Name = "Pressure",
    CurrentValue = 3.5
};

// System automatically matches parameter name to expression
var triggered = await ruleEngineService.EvaluateAsync(parameter, rule);
```

### Example 2: Multi-Parameter Expression
```csharp
// Not yet supported in current EvaluateAsync signature
// But infrastructure is ready for future enhancement
var expression = "Pressure > 3 && Temperature < 70";
var parameters = new Dictionary<string, object>
{
    { "Pressure", 3.5 },
    { "Temperature", 65.0 }
};
var input = ruleEngineService.BuildDynamicInput(parameters);
```

### Example 3: Loading from YAML
```csharp
var aspectReader = new AlarmAspectReader("FeedPump~1.0", typeof(DiagnosticAlarmAspect));
var aspect = aspectReader.LoadDiagnosticAlarmAspect("FeedPump~1.0");

foreach (var profile in aspect.DeviceProfile.DiagnosticProfile)
{
    foreach (var resource in profile.DiagnosticResources)
    {
        Console.WriteLine($"Expression: {resource.Rule?.Expression}");
        Console.WriteLine($"Parameter: {resource.Parameter.Name}");
    }
}
```

### Example 4: Parameter Validation
```csharp
var expression = "Pressure > 3 && Temperature < 70";
var parameters = new Dictionary<string, object>
{
    { "Pressure", 3.5 }
    // Temperature is missing!
};

var (isValid, missing) = ruleEngineService.ValidateParametersForExpression(expression, parameters);
if (!isValid)
{
    Console.WriteLine($"Missing: {string.Join(", ", missing)}"); // Output: Temperature
}
```

## Backward Compatibility

The system maintains backward compatibility:
- Rules with `Min`/`Max` properties still work
- Expressions using `input1` are still supported
- Legacy database records don't need immediate migration
- System automatically detects and handles both formats

## Migration Path

### Phase 1 (Current):
- ✅ Update database expressions to use parameter names
- ✅ Update `BuildExpression()` to use `Expression` property first
- ✅ Update `EvaluateAsync()` to handle parameter names
- ✅ Add helper methods for parameter extraction and validation

### Phase 2 (Future):
- Extend `EvaluateAsync()` to accept multiple parameters
- Add support for complex multi-parameter expressions
- Create API endpoints for expression validation
- Add UI for expression builder with parameter suggestions

### Phase 3 (Future):
- Implement expression templates
- Add expression testing/simulation tools
- Support for custom functions in expressions
- Expression versioning and history

## Testing Checklist

- [x] Build solution successfully
- [x] Updated SQL script with parameter names
- [x] Updated RuleEngineService with new methods
- [x] Updated AlarmAspectReader to load aspects
- [ ] Test rule evaluation with parameter names
- [ ] Test parameter extraction
- [ ] Test parameter validation
- [ ] Test YAML loading with expressions
- [ ] Integration test with MQTT data
- [ ] End-to-end test with worker service

## Key Benefits

1. **Self-Documenting**: Expressions like `Pressure > 3` are immediately understandable
2. **YAML Alignment**: Database expressions match YAML configuration
3. **Type Safety**: Parameter names provide better IDE support and validation
4. **Flexibility**: Support for both simple and complex expressions
5. **Scalability**: Ready for multi-parameter expressions in future
6. **Maintainability**: Easier to debug and modify rules
7. **Validation**: Can check for missing parameters before evaluation

## Notes

- Expression property in `Rule` entity is nullable for backward compatibility
- RulesEngine uses C# lambda expression syntax
- Parameter names are case-sensitive
- All parameter values must be provided before evaluation
- Missing parameters will log warnings and return false
