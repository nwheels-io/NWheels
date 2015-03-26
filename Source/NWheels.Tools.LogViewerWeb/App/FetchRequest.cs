using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Tools.LogViewerWeb.App
{
    public class FetchRequest
    {
        public bool Match(ThreadNodeViewModel thread)
        {
            if ( EnvironmentNames != null && EnvironmentNames.Length > 0 && !EnvironmentNames.Contains(thread.Environment, StringComparer.OrdinalIgnoreCase) )
            {
                return false;
            }

            if ( NodeNames != null && NodeNames.Length > 0 && !NodeNames.Contains(thread.Node, StringComparer.OrdinalIgnoreCase) )
            {
                return false;
            }

            if ( ThreadTypes != null && ThreadTypes.Length > 0 && !ThreadTypes.Contains(thread.ThreadType) )
            {
                return false;
            }

            if ( LogLevels != null && LogLevels.Length > 0 && !LogLevels.Contains(thread.Level) )
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long LastCaptureId { get; set; }
        public string[] EnvironmentNames { get; set; }
        public string[] NodeNames { get; set; }
        public ThreadTaskType[] ThreadTypes { get; set; }
        public LogLevel[] LogLevels { get; set; }
        public int? MaxNumberOfCaptures { get; set; }
    }
}
