using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class ThreadLogUINodeEntity : IThreadLogUINodeEntity
    {
        public void CopyFromSnapshot(ref int id, ThreadLogRecord threadRecord, ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            this.Id = threadRecord + "#" + (++id);
            this.NodeType = GetNodeType(snapshot.Level, snapshot.IsActivity);
            this.Icon = GetNodeIcon(snapshot.Level, snapshot.IsActivity);
            this.Text = BuildSingleLineText(snapshot);
            this.TimeText = GetTimeText(threadRecord, snapshot);
            this.DurationMs = snapshot.Duration;
            this.CpuTimeMs = snapshot.CpuTime;
            this.CpuCycles = snapshot.CpuCycles;

            if (snapshot.NameValuePairs != null)
            {
                this.KeyValues = snapshot.NameValuePairs.Where(nvp => !nvp.IsDetail).Select(nvp => nvp.Name + "=" + nvp.Value.OrDefaultIfNull("")).ToArray();
                this.AdditionalDetails = snapshot.NameValuePairs.Where(nvp => nvp.IsDetail).Select(nvp => nvp.Name + "=" + nvp.Value.OrDefaultIfNull("")).ToArray();
            }

            this.SubNodes = new List<IThreadLogUINodeEntity>();
            
            if (snapshot.SubNodes != null)
            {
                foreach (var subSnapshot in snapshot.SubNodes)
                {
                    var subNode = Framework.NewDomainObject<IThreadLogUINodeEntity>().As<ThreadLogUINodeEntity>();
                    subNode.CopyFromSnapshot(ref id, threadRecord, subSnapshot);
                    this.SubNodes.Add(subNode);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IThreadLogUINodeEntity

        public abstract string Id { get; set; }

        public ThreadLogNodeType NodeType { get; private set; }
        public string Icon { get; private set; }
        public string Text { get; private set; }
        public string TimeText { get; private set; }
        public long DurationMs { get; private set; }
        public long DbDurationMs { get; private set; }
        public long DbCount { get; private set; }
        public long CpuTimeMs { get; private set; }
        public long CpuCycles { get; private set; }
        public string[] KeyValues { get; private set; }
        public string[] AdditionalDetails { get; private set; }
        public ICollection<IThreadLogUINodeEntity> SubNodes { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual ThreadLogNodeType GetNodeType(LogLevel level, bool isActivity)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return (isActivity ? ThreadLogNodeType.ActivitySuccess : ThreadLogNodeType.LogDebug);
                case LogLevel.Verbose:
                    return (isActivity ? ThreadLogNodeType.ActivitySuccess : ThreadLogNodeType.LogVerbose);
                case LogLevel.Info:
                    return (isActivity ? ThreadLogNodeType.ActivitySuccess : ThreadLogNodeType.LogInfo);
                case LogLevel.Warning:
                    return (isActivity ? ThreadLogNodeType.ActivityWarning : ThreadLogNodeType.LogWarning);
                case LogLevel.Error:
                    return (isActivity ? ThreadLogNodeType.ActivityError : ThreadLogNodeType.LogError);
                case LogLevel.Critical:
                    return (isActivity ? ThreadLogNodeType.ActivityCritical : ThreadLogNodeType.LogCritical);
                default:
                    return ThreadLogNodeType.LogDebug;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected virtual string GetNodeIcon(LogLevel level, bool isActivity)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Verbose:
                    return (isActivity ? "check" : null);
                case LogLevel.Info:
                    return (isActivity ? "check" : "info-circle");
                case LogLevel.Warning:
                    return (isActivity ? "exclamation" : "exclamation-triangle");
                case LogLevel.Error:
                case LogLevel.Critical:
                    return (isActivity ? "times" : "times-circle-o");
                default:
                    return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string GetTimeText(ThreadLogRecord threadRecord, ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            return "+ " + snapshot.MillisecondsTimestamp.ToString("#,##0");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string BuildSingleLineText(ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            var messageIdParts = snapshot.MessageId.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var messageIdDisplayPart = (messageIdParts.Length == 2 ? messageIdParts[1] : snapshot.MessageId);

            var text = new StringBuilder();

            text.Append(messageIdDisplayPart.SplitPascalCase());

            if (snapshot.NameValuePairs != null)
            {
                var nonDetailPairs = snapshot.NameValuePairs.Where(p => !p.IsDetail).ToArray();

                if (nonDetailPairs.Length > 0)
                {
                    for (int i = 0; i < nonDetailPairs.Length; i++)
                    {
                        var pair = nonDetailPairs[i];
                        text.AppendFormat("{0}{1}={2}", (i > 0 ? ", " : ": "), pair.Name, pair.Value);
                    }
                }
            }

            return text.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.DependencyProperty]
        protected IFramework Framework { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HandlerExtension : ApplicationEntityService.EntityHandlerExtension<IThreadLogUINodeEntity>
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
