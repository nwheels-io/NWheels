using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogMessageSummaryEntity : ILogMessageSummaryEntity
    {
        public void SetKey(
            string environment, 
            string machine, 
            string node, 
            string instance, 
            string replica, 
            LogLevel? level, 
            string logger, 
            string messageId, 
            string exceptionType)
        {
            this.Environment = environment;
            this.Machine = machine;
            this.Node = node;
            this.Instance = instance;
            this.Replica = replica;
            this.Level = level;
            this.Logger = logger;
            this.MessageId = messageId;
            this.ExceptionType = exceptionType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Increment(LogLevel level, int count)
        {
            this.Count += count;
            
            switch (level)
            {
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
        public LogLevel? Level { get; private set; }
        public string Logger { get; private set; }
        public string MessageId { get; private set; }
        public string ExceptionType { get; private set; }
        public int Count { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }
        public int CriticalCount { get; private set; }
    }
}
