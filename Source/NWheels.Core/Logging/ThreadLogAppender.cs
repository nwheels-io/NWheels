using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    internal class ThreadLogAppender : IThreadLogAppender
    {
        private readonly IFramework _framework;
        private readonly IThreadLogAnchor _anchor;
        private readonly IThreadRegistry _registry;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogAppender(IFramework framework, IThreadLogAnchor anchor, IThreadRegistry registry)
        {
            _framework = framework;
            _anchor = anchor;
            _registry = registry;
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
                using ( var unknownThreadActivity = new FormattedActivityLogNode("???") )
                { 
                    StartThreadLogNoCheck(ThreadTaskType.Unspecified, unknownThreadActivity);
                    _anchor.CurrentThreadLog.AppendNode(node);
                }
            }

            PlainLog.LogNode(node);

            //if ( node.Level >= LogLevel.Warning )
            //{
            //    PlainLog.LogNode(node);
            //}
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
                StartThreadLogNoCheck(ThreadTaskType.Unspecified, activity);
            }

            PlainLog.LogActivity(activity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartThreadLog(ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            var currentLog = _anchor.CurrentThreadLog;

            if ( currentLog != null )
            {
                currentLog.AppendNode(rootActivity);
                PlainLog.LogActivity(rootActivity);
            }
            else
            {
                StartThreadLogNoCheck(taskType, rootActivity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartThreadLogNoCheck(ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            _anchor.CurrentThreadLog = new ThreadLog(_framework, new StopwatchClock(), _registry, _anchor, taskType, rootActivity);
            PlainLog.LogActivity(rootActivity);
        }
    }
}
