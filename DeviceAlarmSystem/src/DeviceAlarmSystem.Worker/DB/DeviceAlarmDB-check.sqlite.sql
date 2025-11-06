BEGIN TRANSACTION;
DROP TABLE IF EXISTS "Alarms";
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
DROP TABLE IF EXISTS "Devices";
CREATE TABLE Devices (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT
, Inhibit INTEGER DEFAULT 0, DeviceType TEXT);
DROP TABLE IF EXISTS "Parameters";
CREATE TABLE Parameters (
    Id TEXT PRIMARY KEY,
    DeviceId TEXT NOT NULL,
    Name TEXT NOT NULL,
    CurrentValue REAL NOT NULL,
    Unit TEXT NOT NULL,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
);
DROP TABLE IF EXISTS "Rules";
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
INSERT INTO "Devices" ("Id","Name","Description","Inhibit","DeviceType") VALUES ('11111111-1111-1111-1111-111111111111','Pump-1','Main process pump',0,'Pump'),
 ('22222222-2222-2222-2222-222222222222','Motor-1','Conveyor motor',0,'Motor'),
 ('33333333-3333-3333-3333-333333333333','Valve-1','Intake valve',0,'Valve');
INSERT INTO "Parameters" ("Id","DeviceId","Name","CurrentValue","Unit") VALUES ('a1111111-1111-1111-1111-111111111111','11111111-1111-1111-1111-111111111111','Pressure',3.5,'bar'),
 ('a2222222-2222-2222-2222-222222222222','11111111-1111-1111-1111-111111111111','Temperature',60.0,'C'),
 ('a3333333-3333-3333-3333-333333333333','22222222-2222-2222-2222-222222222222','Current',10.0,'A'),
 ('a5555555-5555-5555-5555-555555555555','33333333-3333-3333-3333-333333333333','Position',100.0,'%');
INSERT INTO "Rules" ("Id","ParameterId","WorkflowName","Expression","Severity","Priority","Description","RecommendedAction","ComparisonType","Max","Min","Name") VALUES ('b1111111-1111-1111-1111-111111111111','a1111111-1111-1111-1111-111111111111','PumpPressure','input1 > 3','High',1,'High pressure in pump','Check relief valve','GreaterThan',NULL,3.0,'HighPressure'),
 ('b2222222-2222-2222-2222-222222222222','a2222222-2222-2222-2222-222222222222','PumpTemp','input1 > 70','High',1,'High temperature in pump','Check cooling system','GreaterThan',NULL,70.0,'HighTemp'),
 ('b3333333-3333-3333-3333-333333333333','a3333333-3333-3333-3333-333333333333','MotorCurrent','input1 > 12','High',1,'High current in motor','Inspect motor','GreaterThan',NULL,12.0,'HighCurrent'),
 ('b5555555-5555-5555-5555-555555555555','a5555555-5555-5555-5555-555555555555','ValvePosition','input1 < 90','Low',3,'Valve not fully open','Check actuator','LessThan',90.0,NULL,'LowPosition');
COMMIT;
