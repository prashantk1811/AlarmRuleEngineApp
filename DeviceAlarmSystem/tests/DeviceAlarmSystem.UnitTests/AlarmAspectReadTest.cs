using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using DeviceAlarmSystem.Catalog.AlarmAspect;


namespace DeviceAlarmSystem.Tests
{
    public class AlarmAspectReaderTests
    {
        [Fact]
        public async Task CanReadAlarmAspectYaml()
        {
          var reader = new AlarmAspectReader("FeedPump~1.0", typeof(DiagnosticAlarmAspect));

            // Assert
            Assert.NotNull(reader);
        }
    }
}
