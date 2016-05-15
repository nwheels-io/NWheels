using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogLevelSummaryEntity : ILogLevelSummaryEntity
    {
        public void SetKey(string environment, string machine, string node, string instance, string replica)
        {
            this.Environment = environment;
            this.Machine = machine;
            this.Node = node;
            this.Instance = instance;
            this.Replica = replica;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Increment(LogLevel level, int count)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    this.DebugCount += count;
                    break;
                case LogLevel.Verbose:
                    this.VerboseCount += count;
                    break;
                case LogLevel.Info:
                    this.InfoCount += count;
                    break;
                case LogLevel.Warning:
                    this.WarningCount += count;
                    break;
                case LogLevel.Error:
                    this.ErrorCount += count;
                    break;
                case LogLevel.Critical:
                    this.CriticalCount += count;
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string Id { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Machine { get; private set; }
        public string Environment { get; private set; }
        public string Node { get; private set; }
        public string Instance { get; private set; }
        public string Replica { get; private set; }
        public int DebugCount { get; private set; }
        public int VerboseCount { get; private set; }
        public int InfoCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }
        public int CriticalCount { get; private set; }
    }
}
