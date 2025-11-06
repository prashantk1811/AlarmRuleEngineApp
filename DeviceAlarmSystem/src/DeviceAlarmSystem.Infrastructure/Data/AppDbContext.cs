using DeviceAlarmSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceAlarmSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Alarm> Alarms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
