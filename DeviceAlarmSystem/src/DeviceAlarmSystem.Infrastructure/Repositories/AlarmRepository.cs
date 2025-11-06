using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceAlarmSystem.Infrastructure.Repositories
{
    public class AlarmRepository : RepositoryBase<Alarm>, IAlarmRepository
    {
        public AlarmRepository(AppDbContext context) : base(context) { }

        // Example: Get active alarms
        // public async Task<IEnumerable<Alarm>> GetActiveAlarmsAsync()
        // {
        //     return await _context.Alarms.Where(a => a.IsActive).ToListAsync();
        // }
    }
}
