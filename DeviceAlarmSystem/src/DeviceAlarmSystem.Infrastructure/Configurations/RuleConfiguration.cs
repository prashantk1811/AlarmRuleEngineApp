using DeviceAlarmSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceAlarmSystem.Infrastructure.Configurations
{
    public class RuleConfiguration : IEntityTypeConfiguration<Rule>
    {
        public void Configure(EntityTypeBuilder<Rule> builder)
        {
            builder.ToTable("Rules");
            builder.HasKey(r => r.Id);
            // builder.Property(r => r.Description).HasMaxLength(500);
        }
    }
}
