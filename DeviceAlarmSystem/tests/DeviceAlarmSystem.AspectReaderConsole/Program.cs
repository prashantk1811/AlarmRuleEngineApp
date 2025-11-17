using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DeviceAlarmSystem.Catalog.AlarmAspect;

namespace DeviceAlarmSystem.AspectReaderConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //FeedPump~1.0~DiagnosticAlarmAspect

            string deviceType = args.Length > 0 ? args[0] : "FeedPump";
            string deviceversion = args.Length > 0 ? args[1] : "1.0";
            string aspectName = args.Length > 1 ? args[2] : "DiagnosticAlarmAspect";
            var reader = new AlarmAspectReader(string.Format("{0}~{1}",deviceType, deviceversion), typeof(DiagnosticAlarmAspect));


        }
    }
}
