using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Infrastructure.Data;
using DeviceAlarmSystem.RuleEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceAlarmSystem.Worker.Services
{
    public class ParameterMonitorService
    {
        private readonly AppDbContext _dbContext;
        private readonly RuleEvaluator _ruleEvaluator;
        private readonly ILogger<ParameterMonitorService> _logger;

        public ParameterMonitorService(AppDbContext dbContext, RuleEvaluator ruleEvaluator, ILogger<ParameterMonitorService> logger)
        {
            _dbContext = dbContext;
            _ruleEvaluator = ruleEvaluator;
            _logger = logger;
        }

        public async Task ProcessParametersAsync(CancellationToken cancellationToken)
        {
            var parameters = await _dbContext.Parameters
                .Include(p => p.Rules)
                .Include(p => p.Device)
                .ToListAsync(cancellationToken);

            foreach (var parameter in parameters)
            {
                // [ALARM-LOGIC-2025] Skip alarm generation if device is inhibited
                if (parameter.Device != null && parameter.Device.Inhibit)
                {
                    _logger.LogInformation($"[ALARM-LOGIC-2025] Device {parameter.Device.Name} is inhibited. Skipping alarms.");
                    continue;
                }

                foreach (var rule in parameter.Rules ?? Enumerable.Empty<Rule>())
                {
                    // [ALARM-LOGIC-2025] Check for existing ACTIVE alarm
                    var existingAlarm = await _dbContext.Alarms.FirstOrDefaultAsync(a => a.RuleId == rule.Id && a.ParameterId == parameter.Id && a.State == DeviceAlarmSystem.Core.Entities.AlarmState.ACTIVE, cancellationToken);

                    var alarm = await _ruleEvaluator.EvaluateAndGenerateAlarmAsync(parameter, rule);
                    if (alarm != null)
                    {
                        if (existingAlarm == null)
                        {
                            // Log alarm data before insertion
                            _logger.LogInformation($"[ALARM-LOGIC-2025] Attempting to insert alarm: Id={alarm.Id}, RuleId={alarm.RuleId}, ParameterId={alarm.ParameterId}, DeviceId={alarm.DeviceId}, TriggeredAt={alarm.TriggeredAt}, Message={alarm.Message}");
                            try
                            {
                                _dbContext.Alarms.Add(alarm);
                                await _dbContext.SaveChangesAsync(cancellationToken);
                                _logger.LogInformation($"[ALARM-LOGIC-2025] Alarm generated: {alarm.Message}");
                            }
                            catch (DbUpdateException ex)
                            {
                                var inner = ex.InnerException?.Message ?? ex.Message;
                                string fkHint = "";
                                if (inner.Contains("FOREIGN KEY constraint failed"))
                                {
                                    // Try to hint which key is missing
                                    bool deviceMissing = !_dbContext.Devices.Any(d => d.Id == alarm.DeviceId);
                                    bool paramMissing = !_dbContext.Parameters.Any(p => p.Id == alarm.ParameterId);
                                    bool ruleMissing = !_dbContext.Rules.Any(r => r.Id == alarm.RuleId);
                                    fkHint = $"[ALARM-LOGIC-2025] Foreign key missing: " +
                                        (deviceMissing ? $"DeviceId={alarm.DeviceId} " : "") +
                                        (paramMissing ? $"ParameterId={alarm.ParameterId} " : "") +
                                        (ruleMissing ? $"RuleId={alarm.RuleId}" : "");
                                }
                                _logger.LogError(ex, $"[ALARM-LOGIC-2025] Failed to insert alarm. {fkHint}");
                            }
                        }
                        else
                        {
                            // Already active, do not add duplicate
                            _logger.LogInformation($"[ALARM-LOGIC-2025] Alarm already active for RuleId={rule.Id}, ParameterId={parameter.Id}");
                        }
                    }
                    else if (existingAlarm != null && existingAlarm.State == DeviceAlarmSystem.Core.Entities.AlarmState.ACTIVE)
                    {
                        // Condition cleared, update alarm state to RTN
                        existingAlarm.State = DeviceAlarmSystem.Core.Entities.AlarmState.RTN;
                        existingAlarm.IsActive = false;
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation($"[ALARM-LOGIC-2025] Alarm returned to normal for RuleId={rule.Id}, ParameterId={parameter.Id}");
                    }
                }
            }
        }
    }
}
