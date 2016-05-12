using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogLevelSummaryEntity : ILogLevelSummaryEntity
    {
        public void Increment(DailySummaryRecord record, DateTime from, DateTime until)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Randomize(string machine, string environment, string node, string instance, string replica)
        {
            this.Machine = machine;
            this.Environment = environment;
            this.Node = node;
            this.Instance = instance;
            this.Replica = replica;

            var random = new Random();

            this.DebugCount = random.Next(1000);
            this.VerboseCount = this.DebugCount / 2;
            this.InfoCount = this.DebugCount / 3;
            this.WarningCount = this.DebugCount / 4;
            this.ErrorCount = this.DebugCount / 5;
            this.CriticalCount = this.DebugCount / 10;
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
