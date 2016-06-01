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
        private ThreadLogNodeDetails _details;
        private int _treeNodeIndex;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyFromSnapshot(ref int treeNodeIndex, ThreadLogRecord threadRecord, ThreadLogSnapshot.LogNodeSnapshot snapshot)
        {
            _treeNodeIndex = (++treeNodeIndex);

            this.Id = threadRecord.LogId + "#" + _treeNodeIndex;
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
                    subNode.CopyFromSnapshot(ref treeNodeIndex, threadRecord, subSnapshot);
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
        public IList<IThreadLogUINodeEntity> SubNodes { get; private set; }

        #endregion

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

        private static readonly TreeNodeIndexComparer _s_treeNodeIndexComparer = new TreeNodeIndexComparer();

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
    }
}
