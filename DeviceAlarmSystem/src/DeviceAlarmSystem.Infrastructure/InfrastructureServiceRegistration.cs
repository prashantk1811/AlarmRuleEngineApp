using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Infrastructure.Data;
using DeviceAlarmSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceAlarmSystem.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // For SQLite development
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IParameterRepository, ParameterRepository>();
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IAlarmRepository, AlarmRepository>();

            return services;
        }
    }
}
