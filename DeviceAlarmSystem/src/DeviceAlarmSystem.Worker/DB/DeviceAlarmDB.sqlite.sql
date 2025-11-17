BEGIN TRANSACTION;
DROP TABLE IF EXISTS "Alarms";
DROP TABLE IF EXISTS "Rules";
DROP TABLE IF EXISTS "Parameters";
DROP TABLE IF EXISTS "Devices";

CREATE TABLE Alarms (
    Id TEXT PRIMARY KEY,
    RuleId TEXT NOT NULL,
    ParameterId TEXT NOT NULL,
    TriggeredAt TEXT NOT NULL,
    CurrentValue REAL NOT NULL,
    IsActive INTEGER NOT NULL,
    Message TEXT, Description TEXT, DeviceId TEXT, Priority INTEGER, RecommendedAction TEXT, Severity TEXT, WorkflowName TEXT, RuleName TEXT, State TEXT,
    FOREIGN KEY (RuleId) REFERENCES Rules(Id),
    FOREIGN KEY (ParameterId) REFERENCES Parameters(Id),
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
);

CREATE TABLE Devices (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Inhibit INTEGER DEFAULT 0,
    DeviceType TEXT
);

CREATE TABLE Parameters (
    Id TEXT PRIMARY KEY,
    DeviceId TEXT NOT NULL,
    Name TEXT NOT NULL,
    CurrentValue REAL NOT NULL,
    Unit TEXT NOT NULL,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
);

CREATE TABLE Rules (
    Id TEXT PRIMARY KEY,
    ParameterId TEXT NOT NULL,
    WorkflowName TEXT NOT NULL,
    Expression TEXT NOT NULL,
    Severity TEXT NOT NULL,
    Priority INTEGER NOT NULL,
    Description TEXT,
    RecommendedAction TEXT, ComparisonType TEXT, Max REAL, Min REAL, Name TEXT,
    FOREIGN KEY (ParameterId) REFERENCES Parameters(Id)
);

INSERT INTO "Devices" ("Id","Name","Description","Inhibit","DeviceType") VALUES
('11111111-1111-1111-1111-111111111111','Pump-1','Main process pump',0,'Pump'),
('22222222-2222-2222-2222-222222222222','Motor-1','Conveyor motor',0,'Motor'),
('33333333-3333-3333-3333-333333333333','Valve-1','Intake valve',0,'Valve'),
('44444444-4444-4444-4444-444444444444','Sensor-1','Temperature sensor',0,'Sensor'),
('55555555-5555-5555-5555-555555555555','Compressor-1','Air compressor',0,'Compressor'),
('66666666-6666-6666-6666-666666666666','Heater-1','Process heater',0,'Heater'),
('77777777-7777-7777-7777-777777777777','Fan-1','Cooling fan',0,'Fan'),
('88888888-8888-8888-8888-888888888888','Boiler-1','Steam boiler',0,'Boiler'),
('99999999-9999-9999-9999-999999999999','Chiller-1','Water chiller',0,'Chiller'),
('10101010-1010-1010-1010-101010101010','Tank-1','Storage tank',0,'Tank'),
('12121212-1212-1212-1212-121212121212','Unknown','Auto-generated device for test alarms',0,'Unknown');

INSERT INTO "Parameters" ("Id","DeviceId","Name","CurrentValue","Unit") VALUES
('21111111-1111-1111-1111-111111111111','11111111-1111-1111-1111-111111111111','Pressure',2.5,'bar'),
('22222222-2222-2222-2222-222222222222','11111111-1111-1111-1111-111111111111','Temperature',60.0,'C'),
('23333333-3333-3333-3333-333333333333','22222222-2222-2222-2222-222222222222','Current',10.0,'A'),
('24444444-4444-4444-4444-444444444444','22222222-2222-2222-2222-222222222222','Vibration',0.5,'mm/s'),
('25555555-5555-5555-5555-555555555555','33333333-3333-3333-3333-333333333333','Position',100.0,'%'),
('26666666-6666-6666-6666-666666666666','44444444-4444-4444-4444-444444444444','Temperature',85.0,'C'),
('27777777-7777-7777-7777-777777777777','55555555-5555-5555-5555-555555555555','Pressure',7.0,'bar'),
('28888888-8888-8888-8888-888888888888','66666666-6666-6666-6666-666666666666','Temperature',120.0,'C'),
('29999999-9999-9999-9999-999999999999','77777777-7777-7777-7777-777777777777','Speed',1500.0,'rpm'),
('21111112-1111-1111-1111-111111111111','88888888-8888-8888-8888-888888888888','Pressure',10.0,'bar'),
('22222223-2222-2222-2222-222222222222','99999999-9999-9999-9999-999999999999','Temperature',5.0,'C'),
('23333334-3333-3333-3333-333333333333','10101010-1010-1010-1010-101010101010','Level',80.0,'%'),
('21111113-1111-1111-1111-111111111111','11111111-1111-1111-1111-111111111111','Flow',30.0,'L/min'),
('22222224-2222-2222-2222-222222222222','22222222-2222-2222-2222-222222222222','Voltage',400.0,'V'),
('23333335-3333-3333-3333-333333333333','33333333-3333-3333-3333-333333333333','Leakage',0.0,'L/min'),
('24444445-4444-4444-4444-444444444444','44444444-4444-4444-4444-444444444444','Humidity',40.0,'%'),
('25555556-5555-5555-5555-555555555555','55555555-5555-5555-5555-555555555555','Temperature',45.0,'C'),
('26666667-6666-6666-6666-666666666666','66666666-6666-6666-6666-666666666666','Power',5.0,'kW'),
('27777778-7777-7777-7777-777777777777','77777777-7777-7777-7777-777777777777','Current',2.0,'A'),
('28888889-8888-8888-8888-888888888888','88888888-8888-8888-8888-888888888888','Level',60.0,'%');

INSERT INTO "Rules" ("Id","ParameterId","WorkflowName","Expression","Severity","Priority","Description","RecommendedAction","ComparisonType","Max","Min","Name") VALUES
('31111111-1111-1111-1111-111111111111','21111111-1111-1111-1111-111111111111','PumpPressure','Pressure > 3','High',1,'High pressure in pump','Check relief valve','GreaterThan',NULL,3.0,'HighPressure'),
('32222222-2222-2222-2222-222222222222','22222222-2222-2222-2222-222222222222','PumpTemp','Temperature > 70','High',1,'High temperature in pump','Check cooling system','GreaterThan',NULL,70.0,'HighTemp'),
('33333333-3333-3333-3333-333333333333','23333333-3333-3333-3333-333333333333','MotorCurrent','Current > 12','High',1,'High current in motor','Inspect motor','GreaterThan',NULL,12.0,'HighCurrent'),
('34444444-4444-4444-4444-444444444444','24444444-4444-4444-4444-444444444444','MotorVibration','Vibration > 1','Medium',2,'High vibration in motor','Check alignment','GreaterThan',NULL,1.0,'HighVibration'),
('35555555-5555-5555-5555-555555555555','25555555-5555-5555-5555-555555555555','ValvePosition','Position < 90','Low',3,'Valve not fully open','Check actuator','LessThan',90.0,NULL,'LowPosition'),
('36666666-6666-6666-6666-666666666666','26666666-6666-6666-6666-666666666666','SensorTemp','Temperature > 80','High',1,'High temperature detected','Check process','GreaterThan',NULL,80.0,'HighTemp'),
('37777777-7777-7777-7777-777777777777','27777777-7777-7777-7777-777777777777','CompressorPressure','Pressure < 6','Medium',2,'Low pressure in compressor','Check for leaks','LessThan',6.0,NULL,'LowPressure'),
('38888888-8888-8888-8888-888888888888','28888888-8888-8888-8888-888888888888','HeaterTemp','Temperature > 130','High',1,'Heater over temperature','Shut down heater','GreaterThan',NULL,130.0,'OverTemp'),
('39999999-9999-9999-9999-999999999999','29999999-9999-9999-9999-999999999999','FanSpeed','Speed < 1400','Low',3,'Fan speed too low','Check power supply','LessThan',1400.0,NULL,'LowSpeed'),
('31111112-1111-1111-1111-111111111111','21111112-1111-1111-1111-111111111111','BoilerPressure','Pressure > 12','High',1,'Boiler over pressure','Release pressure','GreaterThan',NULL,12.0,'OverPressure'),
('32222223-2222-2222-2222-222222222222','22222223-2222-2222-2222-222222222222','ChillerTemp','Temperature > 8','Medium',2,'Chiller temperature high','Check refrigerant','GreaterThan',NULL,8.0,'HighTemp'),
('33333334-3333-3333-3333-333333333333','23333334-3333-3333-3333-333333333333','TankLevel','Level < 30','Low',3,'Tank level low','Refill tank','LessThan',30.0,NULL,'LowLevel'),
('31111113-1111-1111-1111-111111111111','21111113-1111-1111-1111-111111111111','PumpFlow','Flow < 25','Medium',2,'Low flow in pump','Check for blockage','LessThan',25.0,NULL,'LowFlow'),
('32222224-2222-2222-2222-222222222222','22222224-2222-2222-2222-222222222222','MotorVoltage','Voltage < 380','Medium',2,'Low voltage in motor','Check supply','LessThan',380.0,NULL,'LowVoltage'),
('33333335-3333-3333-3333-333333333333','23333335-3333-3333-3333-333333333333','ValveLeakage','Leakage > 0','High',1,'Valve leakage detected','Repair valve','GreaterThan',NULL,0.0,'Leakage'),
('34444445-4444-4444-4444-444444444444','24444445-4444-4444-4444-444444444444','SensorHumidity','Humidity > 60','Medium',2,'High humidity detected','Check environment','GreaterThan',NULL,60.0,'HighHumidity'),
('35555556-5555-5555-5555-555555555555','25555556-5555-5555-5555-555555555555','CompressorTemp','Temperature > 50','High',1,'Compressor over temperature','Check cooling','GreaterThan',NULL,50.0,'OverTemp'),
('36666667-6666-6666-6666-666666666666','26666667-6666-6666-6666-666666666666','HeaterPower','Power > 6','Medium',2,'Heater power high','Check load','GreaterThan',NULL,6.0,'HighPower'),
('37777778-7777-7777-7777-777777777777','27777778-7777-7777-7777-777777777777','FanCurrent','Current > 3','Medium',2,'Fan current high','Check wiring','GreaterThan',NULL,3.0,'HighCurrent'),
('38888889-8888-8888-8888-888888888888','28888889-8888-8888-8888-888888888888','BoilerLevel','Level < 50','Low',3,'Boiler level low','Refill boiler','LessThan',50.0,NULL,'LowLevel');

COMMIT;