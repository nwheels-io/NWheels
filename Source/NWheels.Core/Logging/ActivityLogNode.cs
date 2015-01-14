//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NWheels.Logging;

//namespace NWheels.Core.Logging
//{
//    internal class ActivityLogNode : LogNode
//    {
//        private readonly IThreadClock _clock;
//        private LogNode _firstChild;
//        private int _millisecondsDuration;
//        private Exception _error;
//        private bool _isClosed;

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public ActivityLogNode(IThreadClock clock, ILazyLogText lazyText)
//            : base(timestamp, LogNodeType.Activity, level, lazyText)
//        {
//            _clock = clock;
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public ActivityLogNode(IThreadClock clock, int timestamp, LogLevel level, ILazyLogText lazyText)
//            : base(timestamp, LogNodeType.Activity, level, lazyText)
//        {
//            _clock = clock;
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public void Close()
//        {
//            _millisecondsDuration = _clock.ElapsedMilliseconds - base.MillisecondsTimestamp;
//            _isClosed = true;
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public LogNode FirstChild
//        {
//            get { return _firstChild; }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public int MillisecondsDuration
//        {
//            get { return _millisecondsDuration; }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public Exception Error
//        {
//            get { return _error; }
//        }


//    }
//}
