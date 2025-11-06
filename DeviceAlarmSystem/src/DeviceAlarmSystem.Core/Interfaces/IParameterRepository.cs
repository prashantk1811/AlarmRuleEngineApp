using DeviceAlarmSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Core.Interfaces
{
    public interface IParameterRepository : IRepositoryBase<Parameter>
    {
        Task<IEnumerable<Parameter>> GetByDeviceIdAsync(Guid deviceId);
        Task<Parameter> GetByNameAndDeviceIdAsync(string name, Guid deviceId);
    }
}
