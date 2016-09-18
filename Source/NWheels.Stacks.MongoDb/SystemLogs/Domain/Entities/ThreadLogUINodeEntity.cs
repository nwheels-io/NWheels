using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ThreadLogRecord _threadRecord;
        private ThreadLogSnapshot.LogNodeSnapshot _logRecord;
        private ThreadLogUINodeEntity _parentNode;
        private ThreadLogNodeDetails _details;
        private int _treeNodeIndex;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyFromLogRecord(
            ref int treeNodeIndex, 
            ThreadLogRecord threadRecord, 
            ThreadLogSnapshot.LogNodeSnapshot logRecord,
            ThreadLogUINodeEntity parentNode)
        {
            _threadRecord = threadRecord;
            _logRecord = logRecord;
            _parentNode = parentNode;
            _treeNodeIndex = (++treeNodeIndex);
    
            this.Id = threadRecord.LogId + "#" + _treeNodeIndex;
            this.MessageId = logRecord.MessageId.TrimStart('!');
            this.Timestamp = threadRecord.Timestamp.AddMilliseconds(logRecord.MicrosecondsTimestamp / 1000);
            this.Level = logRecord.Level;
            this.NodeType = GetNodeType(logRecord.Level, logRecord.IsActivity);
            this.Icon = GetNodeIcon(logRecord.Level, logRecord.IsActivity);
            this.Text = logRecord.BuildSingleLineText();
            this.TimeText = GetTimeText(threadRecord, logRecord);
            this.Exception = logRecord.ExceptionDetails;

            if (logRecord.IsActivity)
            {
                this.DurationMilliseconds = logRecord.MicrosecondsDuration / 1000m;
                this.DbCount = logRecord.DbCount;
                this.DbDurationMilliseconds = logRecord.MicrosecondsDbTime / 1000m;

                if (logRecord.MicrosecondsCpuTime > 0 && logRecord.MicrosecondsCpuTime < logRecord.MicrosecondsDuration)
                {
                    this.CpuTimeMilliseconds = logRecord.MicrosecondsCpuTime / 1000m;
                    this.CpuCycles = logRecord.CpuCycles;
                }
            }

            if (logRecord.NameValuePairs != null)
            {
                this.KeyValues = logRecord.NameValuePairs.Where(nvp => !nvp.IsDetail).Select(nvp => nvp.Name + "=" + nvp.Value.OrDefaultIfNull("")).ToArray();
                this.AdditionalDetails = logRecord.NameValuePairs.Where(nvp => nvp.IsDetail).Select(nvp => nvp.Name + "=" + nvp.Value.OrDefaultIfNull("")).ToArray();
            }

            this.SubNodes = new List<IThreadLogUINodeEntity>();
            
            if (logRecord.SubNodes != null)
            {
                foreach (var subSnapshot in logRecord.SubNodes)
                {
                    var subNode = Framework.NewDomainObject<IThreadLogUINodeEntity>().As<ThreadLogUINodeEntity>();
                    subNode.CopyFromLogRecord(ref treeNodeIndex, threadRecord, subSnapshot, parentNode: this);
                    this.SubNodes.Add(subNode);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IThreadLogUINodeEntity

        public abstract string Id { get; set; }

        public ThreadLogNodeType NodeType { get; private set; }
        public string MessageId { get; private set; }
        public DateTime Timestamp { get; protected set; }
        public LogLevel Level { get; private set; }
        public string Icon { get; private set; }
        public string Text { get; private set; }
        public string TimeText { get; private set; }
        public decimal? DurationMilliseconds { get; private set; }
        public decimal? DbDurationMilliseconds { get; private set; }
        public long? DbCount { get; private set; }
        public decimal? CpuTimeMilliseconds { get; private set; }
        public long? CpuCycles { get; private set; }
        public string Exception { get; private set; }
        public string[] KeyValues { get; private set; }
        public string[] AdditionalDetails { get; private set; }
        public IList<IThreadLogUINodeEntity> SubNodes { get; private set; }

        #endregion
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public decimal? WaitTimeMilliseconds 
        {
            get
            {
                return DurationMilliseconds - CpuTimeMilliseconds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public ThreadLogNodeDetails Details
        {
            get { return _details; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal bool TryFindTreeNodeByIndex(IThreadLogUINodeEntity queryByExample, out ThreadLogUINodeEntity foundNode)
        {
            if (_treeNodeIndex == queryByExample.As<ThreadLogUINodeEntity>()._treeNodeIndex)
            {
                foundNode = this;
                return true;
            }

            var subNodeList = (SubNodes as List<IThreadLogUINodeEntity>);

            if (subNodeList != null)
            {
                var searchIndex = subNodeList.BinarySearch(queryByExample, _s_treeNodeIndexComparer);

                if (searchIndex >= 0)
                {
                    foundNode = subNodeList[searchIndex].As<ThreadLogUINodeEntity>();
                    return true;
                }

                var subSearchIndex = ~searchIndex;

                if (subSearchIndex > 0 && subSearchIndex <= subNodeList.Count)
                {
                    return subNodeList[subSearchIndex - 1].As<ThreadLogUINodeEntity>().TryFindTreeNodeByIndex(queryByExample, out foundNode);
                }
            }

            foundNode = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void BuildDetails()
        {
            _details = new ThreadLogNodeDetails();

            var thisMessage = BuildDetailsMessagePart();

            _details.Message = thisMessage.Message;
            _details.Time = thisMessage.Time;
            _details.Values = thisMessage.Values;
            _details.Session = new ThreadLogNodeDetails.SessionDetails();
            
            _details.AllValues = BuildDetailsAllValuesPart();
            _details.Counters = BuildDetailsCountersPart();
            _details.Exception = _logRecord.ExceptionDetails;
            _details.Stack = BuildDetailsBriefStackPart();
            _details.StackDetailed = BuildDetailsFullStackPart();
            _details.Thread = BuildDetailsThreadPart();
            _details.Process = BuildDetailsProcessPart();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ClearChildren()
        {
            this.KeyValues = null;
            this.AdditionalDetails = null;
            this.SubNodes = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void SetQueryByExample(int treeNodeIndex)
        {
            _treeNodeIndex = treeNodeIndex;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal int GetTreeNodeIndex()
        {
            return _treeNodeIndex;
        }

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
                    return (isActivity ? "check" : "angle-right");
                case LogLevel.Info:
                    return (isActivity ? "check" : "info-circle");
                case LogLevel.Warning:
                    return (isActivity ? "exclamation" : "exclamation-triangle");
                case LogLevel.Error:
                case LogLevel.Critical:
                    return "times";
                default:
                    return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string GetTimeText(ThreadLogRecord threadRecord, ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            return "+ " + ((decimal)snapshot.MicrosecondsTimestamp / 1000m).ToString("#,##0.00");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.DependencyProperty]
        protected IFramework Framework { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogNodeDetails.MessageDetails BuildDetailsMessagePart()
        {
            return new ThreadLogNodeDetails.MessageDetails() {
                Time = string.Format(
                    "{0:yyyy-MM-dd HH:mm:ss.fff} UTC | thread start + {1:#,##0.00} ms",
                    _threadRecord.Timestamp.AddMilliseconds(_logRecord.MicrosecondsTimestamp / 1000),
                    (decimal)_logRecord.MicrosecondsTimestamp / 1000m),
                Message = string.Format(
                    "{0} | {1}:{2}", 
                    this.MessageId, 
                    (_logRecord.IsActivity ? "Activity" : "Log"),
                    this.Level),
                Values = _logRecord.NameValuePairs
                    .Distinct(_s_nameValuePairByNameComparer)
                    .ToDictionary(nvp => nvp.Name, nvp => nvp.Value)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<string, string> BuildDetailsAllValuesPart()
        {
            var values = new Dictionary<string, string>();

            for (var parent = _parentNode; parent != null; parent = parent._parentNode)
            {
                foreach (var nvp in parent._logRecord.NameValuePairs)
                {
                    if (!values.ContainsKey(nvp.Name))
                    {
                        values[nvp.Name] = nvp.Value;
                    }
                }
            }

            return values;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<string, decimal> BuildDetailsCountersPart()
        {
            if (!_logRecord.IsActivity)
            {
                return null;
            }

            var counters = new Dictionary<string, decimal> {
                { "Duration", _logRecord.MicrosecondsDuration },
                { "DbCount", 0 },
                { "DbDurationMs", 0 },
                { "CpuTimeNs", _logRecord.MicrosecondsCpuTime },
                { "CpuCycles", _logRecord.CpuCycles },
            };

            return counters;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string[] BuildDetailsBriefStackPart()
        {
            var stack = new List<string>();

            for (var parent = _parentNode; parent != null; parent = parent._parentNode)
            {
                stack.Add(parent._logRecord.BuildSingleLineText());
            }

            return stack.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogNodeDetails.MessageDetails[] BuildDetailsFullStackPart()
        {
            var stack = new List<ThreadLogNodeDetails.MessageDetails>();

            for (var parent = _parentNode; parent != null; parent = parent._parentNode)
            {
                stack.Add(parent.BuildDetailsMessagePart());
            }

            return stack.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogNodeDetails.ThreadDetails BuildDetailsThreadPart()
        {
            return new ThreadLogNodeDetails.ThreadDetails() {
                StartedAtUtc = _threadRecord.Timestamp,
                TaskType = _threadRecord.TaskType,
                LogId = _threadRecord.LogId,
                CorrelationId = _threadRecord.CorrelationId,
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogNodeDetails.ProcessDetails BuildDetailsProcessPart()
        {
            return new ThreadLogNodeDetails.ProcessDetails() {
                Environment = _threadRecord.EnvironmentName,
                Node = _threadRecord.NodeName,
                Instance = _threadRecord.NodeInstance,
                Replica = _threadRecord.NodeInstanceReplica,
                Machine = _threadRecord.MachineName,
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly TreeNodeIndexComparer _s_treeNodeIndexComparer = new TreeNodeIndexComparer();
        private static readonly NameValuePairByNameComparer _s_nameValuePairByNameComparer = new NameValuePairByNameComparer();

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TreeNodeIndexComparer : IComparer<IThreadLogUINodeEntity>
        {
            #region Implementation of IComparer<in IThreadLogUINodeEntity>

            public int Compare(IThreadLogUINodeEntity x, IThreadLogUINodeEntity y)
            {
                return ((ThreadLogUINodeEntity)x)._treeNodeIndex.CompareTo(((ThreadLogUINodeEntity)y)._treeNodeIndex);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NameValuePairByNameComparer : IEqualityComparer<ThreadLogSnapshot.NameValuePairSnapshot>
        {
            #region Implementation of IEqualityComparer<in NameValuePairSnapshot>

            public bool Equals(ThreadLogSnapshot.NameValuePairSnapshot x, ThreadLogSnapshot.NameValuePairSnapshot y)
            {
                return (String.Compare(x.Name, y.Name, StringComparison.Ordinal) == 0);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int GetHashCode(ThreadLogSnapshot.NameValuePairSnapshot x)
            {
                return x.Name.GetHashCode();
            }

            #endregion
        }
    }
}
