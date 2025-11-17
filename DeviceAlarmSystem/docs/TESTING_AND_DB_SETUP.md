# DeviceAlarmSystem: Testing & Database Setup Guide

## 1. Running Tests

### Unit Tests
- Located in `tests/DeviceAlarmSystem.UnitTests`
- Run with:
  ```sh
  dotnet test tests/DeviceAlarmSystem.UnitTests/DeviceAlarmSystem.UnitTests.csproj
  ```

### Integration Tests
- Located in `tests/DeviceAlarmSystem.IntegrationTests`
- Run with:
  ```sh
  dotnet test tests/DeviceAlarmSystem.IntegrationTests/DeviceAlarmSystem.IntegrationTests.csproj
  ```
- Integration tests use an in-memory or SQLite database for isolation.

## 2. Required Database Entries for Application

To run the application and see alarms generated, you need the following minimal data in your database:

### Device Table
| Id (Guid) | Name           | Description      | Inhibit (bool/int) | DeviceType |
|----------|----------------|-----------------|--------------------|------------|
| (GUID)   | Pump-1         | Main pump unit  | 0                  | Pump       |

### Parameter Table
| Id (Guid) | DeviceId (Guid) | Name        | CurrentValue | Unit |
|-----------|-----------------|-------------|--------------|------|
| (GUID)    | (Device GUID)   | Temperature | 85           | C    |

### Rule Table
| Id (Guid) | ParameterId (Guid) | WorkflowName         | Expression         | Severity | Priority | Description         | RecommendedAction | ComparisonType | Min | Max | Name        |
|-----------|---------------------|----------------------|--------------------|----------|----------|---------------------|-------------------|---------------|-----|-----|-------------|
| (GUID)    | (Parameter GUID)    | TempAlarmWorkflow    | input1 > 80        | High     | 1        | High temp detected  | Check coolant     | GreaterThan   | 80  |     | HighTempRule|

### Alarm Table (auto-generated)
| Id (Guid) | RuleId | ParameterId | TriggeredAt | CurrentValue | IsActive | Message | DeviceId | State (enum) | ... |
|-----------|--------|-------------|-------------|--------------|----------|---------|----------|--------------|-----|
| ...       | ...    | ...         | ...         | ...          | ...      | ...     | ...      | ACTIVE/RTN   | ... |

- Alarms are created by the RuleEngine when a rule is triggered.
- Alarm states are managed as enums: ACTIVE, ACK, RTN, ACKRTN.
- Duplicate ACTIVE alarms are prevented; state transitions to RTN when condition clears.

## 3. Database Schema Updates (SQL)

Add new columns for alarm state, device inhibit, and device type:

```sql
ALTER TABLE Alarms ADD COLUMN State TEXT; -- stores enum as string
ALTER TABLE Devices ADD COLUMN Inhibit INTEGER DEFAULT 0;
ALTER TABLE Devices ADD COLUMN DeviceType TEXT;
```

## 4. Example: Minimal SQL Inserts (SQLite)

```sql
INSERT INTO Devices (Id, Name, Description, Inhibit, DeviceType) VALUES ('<device-guid>', 'Pump-1', 'Main pump unit', 0, 'Pump');
INSERT INTO Parameters (Id, DeviceId, Name, CurrentValue, Unit) VALUES ('<param-guid>', '<device-guid>', 'Temperature', 85, 'C');
INSERT INTO Rules (Id, ParameterId, WorkflowName, Expression, Severity, Priority, Description, RecommendedAction, ComparisonType, Min, Name) VALUES ('<rule-guid>', '<param-guid>', 'TempAlarmWorkflow', 'input1 > 80', 'High', 1, 'High temp detected', 'Check coolant', 'GreaterThan', 80, 'HighTempRule');
```

- Replace `<device-guid>`, `<param-guid>`, `<rule-guid>` with actual GUIDs.

## 5. Alarm State Enum

Alarm states are managed as an enum in code:

```csharp
public enum AlarmState {
    ACTIVE,
    ACK,
    RTN,
    ACKRTN
}
```
- The enum is stored as a string in the database (e.g., 'ACTIVE', 'RTN').

## 6. Application Startup
- Ensure your database is created and seeded with at least one Device, Parameter, and Rule as above.
- Start the API or Worker project:
  ```sh
  dotnet run --project src/DeviceAlarmSystem.Api/DeviceAlarmSystem.Api.csproj
  # or
  dotnet run --project src/DeviceAlarmSystem.Worker/DeviceAlarmSystem.Worker.csproj
  ```
- The worker will evaluate parameters and generate alarms if rules are triggered.
- If a device's Inhibit flag is set, alarms for its parameters will not be generated.
- When a parameter returns to normal, the alarm state will be set to RTN.

## 7. Test Data for Multiple Devices/Alarms
- See the provided SQL scripts in the documentation or ask for a sample script to generate 50+ alarms for robust testing.

---

This guide ensures you can test and run DeviceAlarmSystem with the minimal required data and understand the test structure.
