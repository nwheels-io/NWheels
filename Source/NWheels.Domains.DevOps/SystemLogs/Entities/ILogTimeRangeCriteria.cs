using System;
using NWheels.UI;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [ViewModelContract]
    public interface ILogTimeRangeCriteria
    {
        DateTime From { get; set; }
        DateTime Until { get; set; }
        string MessageId { get; set; }
        int? SeriesIndex { get; set; }
    }
}
