using DeviceAlarmSystem.Core.Entities;
using DeviceAlarmSystem.Core.Interfaces;
using DeviceAlarmSystem.Infrastructure.Data;

namespace DeviceAlarmSystem.Infrastructure.Repositories
{
    public class RuleRepository : RepositoryBase<Rule>, IRuleRepository
    {
        public RuleRepository(AppDbContext context) : base(context) { }
    }
}
