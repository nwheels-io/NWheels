using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogLevelSummaryEntity : ILogLevelSummaryEntity
    {
        #region Implementation of IBaseLogDimensionsEntity

        public abstract string Id { get; set; }
        public abstract string Machine { get; set; }
        public abstract string Environment { get; set; }
        public abstract string Node { get; set; }
        public abstract string Instance { get; set; }
        public abstract string Replica { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILogLevelSummaryEntity

        public abstract int DebugCount { get; set; }
        public abstract int VerboseCount { get; set; }
        public abstract int InfoCount { get; set; }
        public abstract int WarningCount { get; set; }
        public abstract int ErrorCount { get; set; }
        public abstract int CriticalCount { get; set; }

        #endregion
    }
}
