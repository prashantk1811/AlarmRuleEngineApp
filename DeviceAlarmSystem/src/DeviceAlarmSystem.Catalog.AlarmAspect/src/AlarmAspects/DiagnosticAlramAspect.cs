using SchneiderElectric.Automation.Catalog.BaseAspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Catalog.AlarmAspect
{
    /// <summary>
    /// Represents a diagnostic alarm aspect, mapped from YAML.
    /// </summary>
    public class DiagnosticAlarmAspect : Aspect
    {
        /// <summary>
        /// Device profile information.
        /// </summary>
        public required DeviceProfile DeviceProfile { get; set; }

        /// <summary>
        /// Gets the device profile for this aspect.
        /// </summary>
        public DeviceProfile GetDeviceProfile()
        {
            return DeviceProfile;
        }
        
    }

    /// <summary>
    /// Device profile data mapped from YAML.
    /// </summary>
    public class DeviceProfile
    {
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? Inhibit { get; set; }
    public required string Type { get; set; }
    public required int Port { get; set; }
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }
    public required List<DiagnosticProfile> DiagnosticProfile { get; set; }
    }

    /// <summary>
    /// Diagnostic protocol profile.
    /// </summary>
    public class DiagnosticProfile
    {
        public required string DiagnosticProtocol { get; set; }
        public required List<DiagnosticResource> DiagnosticResources { get; set; }
    }

    /// <summary>
    /// Diagnostic resource (alarm definition).
    /// </summary>
    public class DiagnosticResource
    {
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public int? Priority { get; set; }
    public string? RecommendedAction { get; set; }
    public string? Message { get; set; }
    public Rule? Rule { get; set; }
    public required Parameter Parameter { get; set; }
    }

    /// <summary>
    /// Rule definition for an alarm.
    /// </summary>
    public class Rule
    {
    public required string Name { get; set; }
    public string? ComparisonType { get; set; }
    public string? Expression { get; set; }
    }

    /// <summary>
    /// Parameter definition for an alarm.
    /// </summary>
    public class Parameter
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Unit { get; set; }
        public required List<string> Key { get; set; }
    }

    public class DiagnosticParameter
    {
        public required string Name { get; set; }
        public List<string> Attributes { get; set; } = new();
        public List<string> Key { get; set; } = new();
        public string? Unit { get; set; }
    }
}
