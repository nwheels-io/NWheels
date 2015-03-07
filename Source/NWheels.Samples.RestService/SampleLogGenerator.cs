using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;
using System.Threading;
using NWheels.Logging;

namespace NWheels.Samples.RestService
{
    public class SampleLogGenerator : LifecycleEventListenerBase
    {
        private Thread _samplerThread;
        private volatile bool _stopping;



        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _stopping = false;
            _samplerThread = new Thread(RunSamplerThread);
            _samplerThread.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _stopping = true;
            _samplerThread.Join(TimeSpan.FromSeconds(10));
            _samplerThread = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunSamplerThread()
        {
            while ( !_stopping )
            {
                
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogVerbose]
            void ThisIsASampleVerboseMessage();
            [LogDebug]
            void ThisIsASampleDebugMessage();
            [LogInfo]
            void ThisIsASampleInfoMessage();
        }
    }
}
