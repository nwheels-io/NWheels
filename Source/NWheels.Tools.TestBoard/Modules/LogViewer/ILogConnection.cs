using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    public interface ILogConnection : IDisposable
    {
        void StartCapture();
        void StopCapture();
        bool IsCapturing { get; }
        event EventHandler<ThreadLogsCapturedEventArgs> ThreadLogsCaptured;
        event EventHandler<PlainLogsCapturedEventArgs> PlainLogsCaptured;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ThreadLogsCapturedEventArgs : EventArgs
    {
        public ThreadLogsCapturedEventArgs(ThreadLogSnapshot[] threadLogs)
        {
            this.ThreadLogs = threadLogs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogSnapshot[] ThreadLogs { get; private set; } 
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PlainLogsCapturedEventArgs : EventArgs
    {
        public PlainLogsCapturedEventArgs(string[] logLines)
        {
            this.LogLines = logLines;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] LogLines { get; private set; }
    }
}
