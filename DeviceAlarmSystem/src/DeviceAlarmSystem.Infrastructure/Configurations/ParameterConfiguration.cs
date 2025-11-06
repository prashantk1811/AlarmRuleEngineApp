using DeviceAlarmSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceAlarmSystem.Infrastructure.Configurations
{
    public class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
    {
        public void Configure(EntityTypeBuilder<Parameter> builder)
        {
            builder.ToTable("Parameters");
            builder.HasKey(p => p.Id);
            // builder.HasOne<Device>()
            //     .WithMany()
            //     .HasForeignKey(p => p.DeviceId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
