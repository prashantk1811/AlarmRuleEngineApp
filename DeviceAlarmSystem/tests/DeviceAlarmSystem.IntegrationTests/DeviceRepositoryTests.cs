using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using DeviceAlarmSystem.Infrastructure;
using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Infrastructure.Data;
using System;

namespace DeviceAlarmSystem.IntegrationTests
{
    public class DeviceRepositoryTests
    {
        private DbContextOptions<AppDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;
        }

        [Fact]
        public void CanAddAndRetrieveDevice_FromDatabase()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new AppDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            var deviceId = Guid.NewGuid();
            var device = new Device { Id = deviceId, Name = "Test Device" };
            context.Devices.Add(device);
            context.SaveChanges();

            // Act
            var retrieved = context.Devices.Find(deviceId);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved.Name.Should().Be("Test Device");
        }
    }
}
