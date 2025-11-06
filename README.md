# DeviceAlarmSystem

A modular .NET Core solution for device alarm management using Microsoft RulesEngine.

## Structure

- `src/DeviceAlarmSystem.Api/` - ASP.NET Core Web API
- `src/DeviceAlarmSystem.Core/` - Domain models, interfaces, DTOs
- `src/DeviceAlarmSystem.Infrastructure/` - EF Core, Repositories
- `src/DeviceAlarmSystem.RuleEngine/` - RulesEngine wrapper
- `src/DeviceAlarmSystem.Worker/` - Background service
- `tests/DeviceAlarmSystem.Tests/` - Unit/integration tests

## Getting Started

### 1. Restore NuGet packages

```
dotnet restore
```

### 2. Build the solution

```
dotnet build
```

### 3. Set up the database

All required SQL commands to create and seed the database are in:

```
src/DeviceAlarmSystem.Worker/DB/DeviceAlarmDB.sqlite.sql
```

To initialize the SQLite database:
- Open a SQLite client (e.g., DB Browser for SQLite, or command line)
- Execute the contents of `DeviceAlarmDB.sqlite.sql` against your database file (e.g., `DeviceAlarmDB.sqlite`)

### 4. Run the Worker Project

```
dotnet run --project src/DeviceAlarmSystem.Worker/DeviceAlarmSystem.Worker.csproj
```

This will start the background service that monitors parameters and generates alarms.

## NuGet Packages

- [RulesEngine](https://www.nuget.org/packages/RulesEngine)
- [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite)

## Notes

- All projects are scaffolded with minimal code. Implement logic as needed.
- For troubleshooting, check logs in the Worker output and ensure your database is correctly initialized.
