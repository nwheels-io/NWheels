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
        private readonly IThreadLogAnchor _anchor;
        private readonly IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogAppender(IThreadLogAnchor anchor, IComponentContext components)
        {
            _anchor = anchor;
            _components = components;
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
                _anchor.CurrentThreadLog = _components.Resolve<ThreadLog>();
            }
        }
    }
}
