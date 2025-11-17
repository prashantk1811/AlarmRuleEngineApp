using System;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Core.Interfaces
{
    public interface IMqttMonitor
    {
        Task StartMonitoringAsync();
        Task StopMonitoringAsync();
        Task PublishAlarmAsync(Guid deviceId, Guid parameterId, Guid alarmId, string alarmData);
        Task PublishAlarmAcknowledgementAsync(Guid deviceId, Guid parameterId, Guid alarmId, string ackData);
    }
}
