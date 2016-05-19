using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.UI.Toolbox;

namespace NWheels.Domains.DevOps.SystemLogs.UI
{
    internal static class Helpers
    {
        public static void ConfigureLogTimeRange(this Form<ILogTimeRangeCriteria> form)
        {
            form.Range(
            "TimeRange",
            x => x.From,
            x => x.Until,
            TimeRangePreset.Today,
            TimeRangePreset.Yesterday,
            TimeRangePreset.ThisWeek,
            TimeRangePreset.LastHour,
            TimeRangePreset.Last3Hours,
            TimeRangePreset.Last6Hours,
            TimeRangePreset.Last12Hours,
            TimeRangePreset.Last24Hours,
            TimeRangePreset.Last3Days,
            TimeRangePreset.Last7Days);
        }
    }
}
