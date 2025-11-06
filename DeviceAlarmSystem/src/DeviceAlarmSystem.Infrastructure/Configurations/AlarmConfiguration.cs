using System;
using DeviceAlarmSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceAlarmSystem.Infrastructure.Configurations
{
    public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
    {
        public void Configure(EntityTypeBuilder<Alarm> builder)
        {
            builder.ToTable("Alarms");
            builder.HasKey(a => a.Id);
            // builder.Property(a => a.Message).HasMaxLength(500);

            // [ALARM-LOGIC-2025] Store AlarmState enum as string in DB
            builder.Property(a => a.State)
                .HasConversion(
                    v => v.ToString(),
                    v => (DeviceAlarmSystem.Core.Entities.AlarmState)Enum.Parse(typeof(DeviceAlarmSystem.Core.Entities.AlarmState), v)
                );
        }
    }
}
