using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    internal class ThreadLogAppender : IThreadLogAppender
    {
        private readonly IFramework _framework;
        private readonly IThreadLogAnchor _anchor;
        private readonly ThreadLogRegistry _registry;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogAppender(IFramework framework, IThreadLogAnchor anchor, ThreadLogRegistry registry)
        {
            _anchor = anchor;
            _registry = registry;
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendLogNode(LogNode node)
        {
            var currentLog = _anchor.CurrentThreadLog;

            if ( currentLog != null )
            {
                currentLog.AppendNode(node);
            }
            else
            {
                //TODO: create an ad-hoc thread log for one node and close it immediately
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendActivityNode(ActivityLogNode activity)
        {
            var currentLog = _anchor.CurrentThreadLog;

            if ( currentLog != null )
            {
                currentLog.AppendNode(activity);
            }
            else
            {
                _anchor.CurrentThreadLog = new ThreadLog(_framework, _registry, activity.TaskType, rootActivity: activity);
            }
        }
    }
}
