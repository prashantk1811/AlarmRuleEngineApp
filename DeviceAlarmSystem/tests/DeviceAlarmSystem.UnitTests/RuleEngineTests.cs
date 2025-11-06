using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.RuleEngine.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DeviceAlarmSystem.UnitTests
{
    public class RuleEngineTests
    {
        [Fact]
        public async Task EvaluateAsync_ShouldReturnTrue_WhenParameterTriggersRule()
        {
            // Arrange
            var mockRuleRepo = new Mock<IRuleRepository>();
            var mockLogger = new Mock<ILogger<RuleEngineService>>();

            // Sample rule
            var rule = new Rule
            {
                Id = Guid.NewGuid(),
                Name = "TestRule",
                Min = 10,
                Max = 20,
                ComparisonType = "Range"
            };

            mockRuleRepo.Setup(r => r.GetAllAsync())
                        .ReturnsAsync(new List<Rule> { rule });

            var ruleEngineService = new RuleEngineService(mockRuleRepo.Object, mockLogger.Object);

            // Sample parameter that triggers the rule
            var parameter = new DeviceAlarmSystem.Core.Entities.Parameter
            {
                Id = Guid.NewGuid(),
                Name = "Temperature",
                CurrentValue = 15 // within 10-20, should trigger
            };

            await ruleEngineService.LoadRulesFromDatabaseAsync();

            // Act
            var result = await ruleEngineService.EvaluateAsync(parameter, rule);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalse_WhenParameterDoesNotTriggerRule()
        {
            // Arrange
            var mockRuleRepo = new Mock<IRuleRepository>();
            var mockLogger = new Mock<ILogger<RuleEngineService>>();

            var rule = new Rule
            {
                Id = Guid.NewGuid(),
                Name = "TestRule",
                Min = 10,
                Max = 20,
                ComparisonType = "Range"
            };

            mockRuleRepo.Setup(r => r.GetAllAsync())
                        .ReturnsAsync(new List<Rule> { rule });

            var ruleEngineService = new RuleEngineService(mockRuleRepo.Object, mockLogger.Object);

            var parameter = new DeviceAlarmSystem.Core.Entities.Parameter
            {
                Id = Guid.NewGuid(),
                Name = "Temperature",
                CurrentValue = 25 // outside 10-20, should not trigger
            };

            await ruleEngineService.LoadRulesFromDatabaseAsync();

            // Act
            var result = await ruleEngineService.EvaluateAsync(parameter, rule);

            // Assert
            Assert.False(result);
        }
    }
}
