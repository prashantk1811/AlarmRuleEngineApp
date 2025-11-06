using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Infrastructure.Repositories
{
    public class ParameterRepository : RepositoryBase<Parameter>, IParameterRepository
    {
        public ParameterRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Parameter>> GetByDeviceIdAsync(Guid deviceId)
        {
            return await _dbSet.Where(p => p.DeviceId == deviceId).ToListAsync();
        }

        public async Task<Parameter> GetByNameAndDeviceIdAsync(string name, Guid deviceId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name && p.DeviceId == deviceId);
        }
    }
}
