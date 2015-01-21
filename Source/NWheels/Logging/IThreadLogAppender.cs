using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    public interface IThreadLogAppender
    {
        void StartThreadLog(ThreadTaskType taskType, ActivityLogNode rootActivity);
        void AppendLogNode(LogNode node);
        void AppendActivityNode(ActivityLogNode activity);
    }
}
