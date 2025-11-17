namespace DeviceAlarmSystem.Core.Interfaces
{
    public interface IDeviceParameterValueProvider
    {
        double? GetCurrentValue(string deviceId, string parameterName);
    }
}
