using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    [Export(typeof(LogViewerViewModel))]
    public class LogViewerViewModel : Document
    {
        private readonly ControllerBase _controller;
        private readonly List<ILogConnection> _connections;
        private readonly string _explorerItemPath;
        private readonly LogPanelViewModel _logs;
        private bool _captureMode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogViewerViewModel(string explorerItemPath, ControllerBase controller)
            : this(explorerItemPath, controller.CreateLogConnections())
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogViewerViewModel(string explorerItemPath, IEnumerable<ILogConnection> connections)
        {
            _explorerItemPath = explorerItemPath;

            _connections = new List<ILogConnection>();
            _connections.AddRange(connections);

            foreach ( var connection in _connections )
            {
                connection.ThreadLogsCaptured += OnThreadLogsCaptured;
            }

            _logs = new LogPanelViewModel();
            
            _captureMode = true;
            StartCapture();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShouldReopenOnStart
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Logs - " + _explorerItemPath; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ControllerBase Controller
        {
            get { return _controller; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogPanelViewModel Logs
        {
            get { return _logs; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void TryClose(bool? dialogResult = null)
        {
            foreach ( var connection in _connections )
            {
                connection.Dispose();
            }

            base.TryClose(dialogResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CaptureMode
        {
            get
            {
                return _captureMode;
            }
            set
            {
                if ( value != _captureMode )
                {
                    _captureMode = value;

                    if ( _captureMode )
                    {
                        StartCapture();
                    }
                    else
                    {
                        StopCapture();
                    }

                    base.NotifyOfPropertyChange(() => this.CaptureMode);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartCapture()
        {
            foreach ( var connection in _connections )
            {
                connection.StartCapture();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StopCapture()
        {
            foreach ( var connection in _connections )
            {
                connection.StopCapture();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnThreadLogsCaptured(object sender, ThreadLogsCapturedEventArgs args)
        {
            foreach ( var log in args.ThreadLogs )
            {
                Logs.AddLog(log);
            }
        }
    }
}
