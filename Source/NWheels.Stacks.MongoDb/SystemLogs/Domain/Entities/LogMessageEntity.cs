using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Entities;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogMessageEntity : ILogMessageEntity
    {
        public void CopyFromRecord(LogMessageRecord record)
        {
            this.Id = record.Id.ToString();
            this.Machine = record.MachineName;
            this.Environment = record.EnvironmentName;
            this.Node = record.NodeName;
            this.Instance = record.NodeInstance;
            this.Replica = record.NodeInstanceReplica;
            this.Timestamp = record.Timestamp;
            this.Level = record.Level;
            this.Logger = record.Logger;
            this.MessageId = record.MessageId;
            this.DurationMs = record.DurationMs;
            this.ExceptionType = record.ExceptionType;
            this.ExceptionDetails = record.ExceptionDetails;
            this.ThreadLogId = record.ThreadLogId;
            this.CorrelationId = record.CorrelationId;
            this.KeyValues = record.KeyValues;
            this.AdditionalDetails = record.AdditionalDetails;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IBaseLogDimensionsEntity

        public abstract string Id { get; set; }

        public string Machine { get; private set; }
        public string Environment { get; private set; }
        public string Node { get; private set; }
        public string Instance { get; private set; }
        public string Replica { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILogMessageEntity

        public DateTime Timestamp { get; private set; }
        public LogLevel Level { get; private set; }
        public string Logger { get; private set; }
        public string MessageId { get; private set; }
        public string AlertId { get; private set; }
        public long? DurationMs { get; private set; }
        public string ExceptionType { get; private set; }
        public string ExceptionDetails { get; private set; }
        public string ThreadLogId { get; private set; }
        public string CorrelationId { get; private set; }
        public string[] KeyValues { get; private set; }
        public string[] AdditionalDetails { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HandlerExtension : ApplicationEntityService.EntityHandlerExtension<ILogMessageEntity>
        {
            public override bool CanOpenNewUnitOfWork(object txViewModel)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IUnitOfWork OpenNewUnitOfWork(object txViewModel)
            {
                return null;
            }
        }
    }
}
