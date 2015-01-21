using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Testing
{
    public class TestThreadLogAppender : IThreadLogAppender
    {
        private readonly List<LogNode> _logNodes = new List<LogNode>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendLogNode(LogNode node)
        {
            _logNodes.Add(node);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendActivityNode(ActivityLogNode activity)
        {
            _logNodes.Add(activity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartThreadLog(ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            this.StartedThreadLogIndex = _logNodes.Count;
            this.StartedThreadTaskType = taskType;

            _logNodes.Add(rootActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] GetLog()
        {
            return _logNodes.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] TakeLog()
        {
            var result = _logNodes.ToArray();

            _logNodes.Clear();
            this.StartedThreadLogIndex = null;
            this.StartedThreadTaskType = null;
            
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetLogStrings()
        {
            return GetLog().Select(node => node.SingleLineText).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] TakeLogStrings()
        {
            return TakeLog().Select(node => node.SingleLineText).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int? StartedThreadLogIndex { get; private set; }
        public ThreadTaskType? StartedThreadTaskType { get; private set; }
    }
}
