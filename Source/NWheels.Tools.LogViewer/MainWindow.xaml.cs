using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NWheels.Logging;
using System.Threading;
using System.Windows.Threading;

namespace NWheels.Tools.LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private FileSystemWatcher _watcher;
        private DispatcherTimer _displayTimer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            
            _viewModel = new MainViewModel();
            _viewModel.LogFolder = @"C:\Temp\TestLogs";
            _viewModel.ShouldWatchChanged += OnShouldWatchChanged;
            _viewModel.LogFolderChanged += OnLogFolderChanged;

            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = TimeSpan.FromSeconds(2);
            _displayTimer.Tick += OnDisplayTimerTick;
            _displayTimer.Start();

            this.DataContext = _viewModel;

            OnLogFolderChanged(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ResetFileWatcher()
        {
            if ( _watcher != null )
            {
                _watcher.Dispose();
            }

            _watcher = null;

            var canWatch = (_viewModel.ShouldWatch && !string.IsNullOrEmpty(_viewModel.LogFolder) && Directory.Exists(_viewModel.LogFolder));

            if ( canWatch )
            {
                _watcher = new System.IO.FileSystemWatcher();
                _watcher.Path = _viewModel.LogFolder;
                _watcher.NotifyFilter = System.IO.NotifyFilters.FileName;
                _watcher.Filter = "*.threadlog";
                _watcher.Created += new System.IO.FileSystemEventHandler(OnFileCreated);
                _watcher.EnableRaisingEvents = true;
            }
            else
            {
                _viewModel.ShouldWatch = false;
            }

            _viewModel.IsWatching = canWatch;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnDisplayTimerTick(object sender, EventArgs e)
        {
            _viewModel.Logs.DisplayPendingLogs();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void OnLogFolderChanged(object sender, EventArgs e)
        {
            if ( Directory.Exists(_viewModel.LogFolder) )
            {
                _viewModel.Logs.Clear();
                _viewModel.Logs.AddLogsFromFolder(_viewModel.LogFolder);
            }

            ResetFileWatcher();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void OnFileCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            if ( e.ChangeType == WatcherChangeTypes.Created )
            {
                _viewModel.Logs.AddLogFromFile(e.FullPath);
            }
        }
 
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnShouldWatchChanged(object sender, EventArgs e)
        {
            ResetFileWatcher();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnFolderButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedPath = _viewModel.LogFolder;
            
            if ( WinFormsDialogs.BrowseFolder(this, ref selectedPath) )
            {
                _viewModel.LogFolder = selectedPath;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MainViewModel : INotifyPropertyChanged
        {
            private bool _shouldWatch;
            private bool _isWatching;
            private string _logFolder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MainViewModel()
            {
                this.Logs = new LogPanelViewModel();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ShouldWatch
            {
                get
                {
                    return _shouldWatch;
                }
                set
                {
                    if ( value == _shouldWatch )
                    {
                        return;
                    }
                 
                    _shouldWatch = value;

                    if ( ShouldWatchChanged != null )
                    {
                        ShouldWatchChanged(this, EventArgs.Empty);
                    }

                    if ( PropertyChanged != null )
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ShouldWatch"));
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsWatching
            {
                get
                {
                    return _isWatching;
                }
                set
                {
                    if ( value == _isWatching )
                    {
                        return;
                    }

                    _isWatching = value;

                    if ( PropertyChanged != null )
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsWatching"));
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public string LogFolder
            {
                get
                {
                    return _logFolder;
                }
                set
                {
                    if ( value == _logFolder )
                    {
                        return;
                    }

                    _logFolder = value;

                    if ( LogFolderChanged != null )
                    {
                        LogFolderChanged(this, EventArgs.Empty);
                    }

                    if ( PropertyChanged != null )
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LogFolder"));
                        PropertyChanged(this, new PropertyChangedEventArgs("LogFolderExists"));
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool LogFolderExists
            {
                get
                {
                    return Directory.Exists(_logFolder);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public LogPanelViewModel Logs { get; private set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public event PropertyChangedEventHandler PropertyChanged;
            public event EventHandler ShouldWatchChanged;
            public event EventHandler LogFolderChanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false

        private class MainViewModel : DependencyObject
        {
            public MainViewModel()
            {
                this.Logs = new LogPanelViewModel();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsWatching
            {
                get
                {
                    return (bool)GetValue(IsWatchingProperty);
                }
                set
                {
                    SetValue(IsWatchingProperty, value);

                    if ( IsWatchingChanged != null )
                    {
                        IsWatchingChanged(this, EventArgs.Empty);
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public string LogFolder
            {
                get
                {
                    return (string)GetValue(LogFolderProperty);
                }
                set
                {
                    SetValue(LogFolderProperty, value);

                    if ( LogFolderChanged != null )
                    {
                        LogFolderChanged(this, EventArgs.Empty);
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            public LogPanelViewModel Logs { get; private set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public event EventHandler IsWatchingChanged;
            public event EventHandler LogFolderChanged;

            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public static readonly DependencyProperty IsWatchingProperty = DependencyProperty.Register(
                "IsWatching",
                typeof(bool),
                typeof(MainViewModel),
                new FrameworkPropertyMetadata(defaultValue: true));

            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public static readonly DependencyProperty LogFolderProperty = DependencyProperty.Register(
                "LogFolder",
                typeof(string),
                typeof(MainViewModel),
                new FrameworkPropertyMetadata(defaultValue: ""));
        }

        #endif
    }
}
