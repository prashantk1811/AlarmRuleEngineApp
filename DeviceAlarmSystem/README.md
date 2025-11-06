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

1. Restore NuGet packages
2. Build the solution
3. Run the API project

## NuGet Packages
- [RulesEngine](https://www.nuget.org/packages/RulesEngine)

## Notes
- All projects are scaffolded with minimal code. Implement logic as needed.
