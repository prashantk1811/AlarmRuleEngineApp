using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Infrastructure.Data;

namespace DeviceAlarmSystem.Infrastructure.Repositories
{
    public class DeviceRepository : RepositoryBase<Device>, IDeviceRepository
    {
        public DeviceRepository(AppDbContext context) : base(context) { }
    }
}
