
DELETE FROM Alarms;
DELETE FROM Rules;
DELETE FROM Parameters;
DELETE FROM Devices;

-- Insert a single device
INSERT INTO Devices (Id, Name, Description, Inhibit, DeviceType) VALUES
  ('11111111-1111-1111-1111-111111111111', 'Pump-1', 'Main process pump', 0, 'Pump'),
 -- ('22222222-2222-2222-2222-222222222222', 'Motor-1', 'Conveyor motor', 0, 'Motor'),
 -- ('33333333-3333-3333-3333-333333333333', 'Valve-1', 'Intake valve', 0, 'Valve');

-- Insert a single parameter for the device
INSERT INTO Parameters (Id, DeviceId, Name, CurrentValue, Unit) VALUES
  ('a1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 'Pressure', 2.5, 'bar'),
  ('a2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'Temperature', 60.0, 'C'),
 -- ('a3333333-3333-3333-3333-333333333333', '22222222-2222-2222-2222-222222222222', 'Current', 10.0, 'A'),
 -- ('a5555555-5555-5555-5555-555555555555', '33333333-3333-3333-3333-333333333333', 'Position', 100.0, '%');

-- Insert a single rule for the parameter
INSERT INTO Rules (Id, ParameterId, WorkflowName, Expression, Severity, Priority, Description, RecommendedAction, ComparisonType, Max, Min, Name) VALUES
  ('b1111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111', 'PumpPressure', 'input1 > 3', 'High', 1, 'High pressure in pump', 'Check relief valve', 'GreaterThan', NULL, 3.0, 'HighPressure'),
  ('b2222222-2222-2222-2222-222222222222', 'a2222222-2222-2222-2222-222222222222', 'PumpTemp', 'input1 > 70', 'High', 1, 'High temperature in pump', 'Check cooling system', 'GreaterThan', NULL, 70.0, 'HighTemp'),
 -- ('b3333333-3333-3333-3333-333333333333', 'a3333333-3333-3333-3333-333333333333', 'MotorCurrent', 'input1 > 12', 'High', 1, 'High current in motor', 'Inspect motor', 'GreaterThan', NULL, 12.0, 'HighCurrent'),
 -- ('b5555555-5555-5555-5555-555555555555', 'a5555555-5555-5555-5555-555555555555', 'ValvePosition', 'input1 < 90', 'Low', 3, 'Valve not fully open', 'Check actuator', 'LessThan', 90.0, NULL, 'LowPosition');