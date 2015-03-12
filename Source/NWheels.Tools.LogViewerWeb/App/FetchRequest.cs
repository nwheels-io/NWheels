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
        public long LastCaptureId { get; set; }
        public string[] EnvironmentNames { get; set; }
        public string[] NodeNames { get; set; }
        public ThreadTaskType[] ThreadTypes { get; set; }
        public LogLevel[] LogLevels { get; set; }
        public int? MaxNumberOfCaptures { get; set; }
    }
}
