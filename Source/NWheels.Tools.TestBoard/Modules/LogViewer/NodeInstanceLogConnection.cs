using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    internal class NodeInstanceLogConnection : ILogConnection
    {
        private readonly object _syncRoot = new object();
        private readonly List<IReadOnlyThreadLog> _capturedThreadLogs = new List<IReadOnlyThreadLog>();
        private readonly NodeInstanceController _controller;
        private volatile Timer _notificationTimer = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeInstanceLogConnection(NodeInstanceController controller)
        {
            _controller = controller;
            _controller.InjectingComponents += OnInjectingHostComponents;
            _controller.CurrentStateChanged += OnControllerStateChanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            StopCapture();

            _controller.InjectingComponents -= OnInjectingHostComponents;
            _controller.CurrentStateChanged -= OnControllerStateChanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartCapture()
        {
            lock ( _syncRoot )
            {
                if ( _notificationTimer == null )
                {
                    _notificationTimer = new Timer(OnNotificationTimerTick, state: null, dueTime: 500, period: 500);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StopCapture()
        {
            lock ( _syncRoot )
            {
                if ( _notificationTimer != null )
                {
                    _notificationTimer.Dispose();
                    _notificationTimer = null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsCapturing
        {
            get
            {
                return (_notificationTimer != null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<ThreadLogsCapturedEventArgs> ThreadLogsCaptured;
        public event EventHandler<PlainLogsCapturedEventArgs> PlainLogsCaptured;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnInjectingHostComponents(object sender, ComponentInjectionEventArgs args)
        {
            var capture = new ThreadLogPersistor(this);
            args.ContainerBuilder.RegisterInstance(capture).As<IThreadLogPersistor>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddCapturedThreadLog(IReadOnlyThreadLog threadLog)
        {
            if ( IsCapturing )
            {
                lock ( _syncRoot )
                {
                    _capturedThreadLogs.Add(threadLog);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IReadOnlyThreadLog[] TakeCapturedThreadLogs()
        {
            lock ( _syncRoot )
            {
                var result = _capturedThreadLogs.ToArray();
                _capturedThreadLogs.Clear();
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnNotificationTimerTick(object state)
        {
            var capturedThreadLogs = TakeCapturedThreadLogs();

            if ( capturedThreadLogs.Length > 0 && ThreadLogsCaptured != null )
            {
                var capturedThreadLogSnapshots = capturedThreadLogs.Select(log => log.TakeSnapshot()).ToArray();
                ThreadLogsCaptured(this, new ThreadLogsCapturedEventArgs(capturedThreadLogSnapshots));
            }

            if ( PlainLogsCaptured != null )
            {
                PlainLogsCaptured(this, new PlainLogsCapturedEventArgs(new string[0]));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnControllerStateChanged(object sender, ControllerStateEventArgs args)
        {
            //if ( args.State == NodeState.Down )
            //{
                
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadLogPersistor : IThreadLogPersistor
        {
            private readonly NodeInstanceLogConnection _ownerConnection;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogPersistor(NodeInstanceLogConnection ownerConnection)
            {
                _ownerConnection = ownerConnection;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Persist(IReadOnlyThreadLog threadLog)
            {
                _ownerConnection.AddCapturedThreadLog(threadLog);
            }
        }
    }
}
