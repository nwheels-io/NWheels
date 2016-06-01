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
    public abstract class RootThreadLogUINodeEntity : ThreadLogUINodeEntity, IRootThreadLogUINodeEntity
    {
        public void CopyFormThreadRecord(ThreadLogRecord record)
        {
            this.LogId = record.LogId;
            this.CorrelationId = record.CorrelationId;
            this.Machine = record.MachineName;
            this.Environment = record.EnvironmentName;
            this.Node = record.NodeName;
            this.Instance = record.NodeInstance;
            this.Replica = record.NodeInstanceReplica;
            this.Timestamp = record.Timestamp;
            this.TaskType = record.TaskType;

            var treeNodeIndex = 0;
            this.CopyFromLogRecord(ref treeNodeIndex, record, record.Snapshot.RootActivity, parentNode: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRootThreadLogUINodeEntity

        public string LogId { get; private set; }
        public string CorrelationId { get; private set; }
        public string Machine { get; private set; }
        public string Environment { get; private set; }
        public string Node { get; private set; }
        public string Instance { get; private set; }
        public string Replica { get; private set; }
        public DateTime Timestamp { get; private set; }
        public ThreadTaskType TaskType { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Overrides of ThreadLogUINodeEntity

        protected override ThreadLogNodeType GetNodeType(LogLevel level, bool isActivity)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    return ThreadLogNodeType.ThreadWarning;
                case LogLevel.Error:
                    return ThreadLogNodeType.ThreadError;
                case LogLevel.Critical:
                    return ThreadLogNodeType.ThreadCritical;
                default:
                    return ThreadLogNodeType.ThreadSuccess;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override string GetNodeIcon(LogLevel level, bool isActivity)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Verbose:
                case LogLevel.Info:
                    return "check-square";
                case LogLevel.Warning:
                    return "exclamation-triangle";
                case LogLevel.Error:
                case LogLevel.Critical:
                    return "times-circle";
                default:
                    return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override string GetTimeText(ThreadLogRecord threadRecord, ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            return threadRecord.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string BuildSingleLineText()
        {
            return base.BuildSingleLineText();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RootHandlerExtension : ApplicationEntityService.EntityHandlerExtension<IRootThreadLogUINodeEntity>
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
