using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Tools.LogViewerWeb.App
{
    public class FetchResponse
    {
        public long LastCaptureId { get; set; }
        public ThreadLogSnapshot[] Logs { get; set; }
    }
}
