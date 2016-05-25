using System;
using NWheels.UI;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [ViewModelContract]
    public interface IThreadLogSearchCriteria
    {
        string Id { get; set; }
        string CorrelationId { get; set; }
        //bool FindRelated { get; set; }
        //bool FindConcurrent { get; set; }
    }
}
