using SchneiderElectric.Automation.Catalog.Access;
using SchneiderElectric.Automation.Catalog.BaseAspects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Catalog.AlarmAspect
{
    public class AlarmAspectReader
    {

        private readonly Manager manager;
        private readonly FileAspectReader fileAspect;

        public AlarmAspectReader(string aspectName, Type aspectType, string contentFolderName = "Content")
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var catalogDataPath = Path.Combine(currentPath!, contentFolderName);
            catalogDataPath = Path.GetFullPath(catalogDataPath);
            var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(catalogDataPath);
            var aspectAssembliesProvider = new DefaultAspectAssembliesProvider([aspectType.Assembly]);
            var memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            fileAspect = new FileAspectReader(fileProvider, memoryCache);
            fileAspect.Initialize([aspectType]);
            manager = new Manager(memoryCache, fileAspect, aspectAssembliesProvider);
        }

        /// <summary>
        /// Load a DiagnosticAlarmAspect from YAML
        /// </summary>
        /// <param name="aspectName">The aspect name (e.g., "FeedPump~1.0")</param>
        /// <returns>The loaded DiagnosticAlarmAspect</returns>
        public DiagnosticAlarmAspect? LoadDiagnosticAlarmAspect(string aspectName)
        {
            return fileAspect.LoadAspect(aspectName, typeof(DiagnosticAlarmAspect)) as DiagnosticAlarmAspect;
        }

        /// <summary>
        /// Check if aspect exists
        /// </summary>
        /// <param name="aspectName">The aspect name</param>
        /// <param name="aspectTypeName">The aspect type name</param>
        /// <returns>True if aspect exists</returns>
        public bool HasAspect(string aspectName, string aspectTypeName)
        {
            return manager.HasAspect(aspectName, aspectTypeName);
        }

    }
}
