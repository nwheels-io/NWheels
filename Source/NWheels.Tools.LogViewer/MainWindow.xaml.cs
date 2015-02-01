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

namespace NWheels.Tools.LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private FileSystemWatcher _watcher;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            
            _viewModel = new MainViewModel();
            _viewModel.IsWatchingChanged += OnIsWatchingChanged;
            _viewModel.LogFolderChanged += OnLogFolderChanged;
            this.DataContext = _viewModel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ResetFileWatcher()
        {
            if ( _watcher != null )
            {
                _watcher.Dispose();
            }

            _watcher = null;

            if ( _viewModel.IsWatching && !string.IsNullOrEmpty(_viewModel.LogFolder) && Directory.Exists(_viewModel.LogFolder) )
            {
                _watcher = new System.IO.FileSystemWatcher();
                _watcher.Path = _viewModel.LogFolder;
                _watcher.NotifyFilter = System.IO.NotifyFilters.FileName;
                _watcher.Filter = "*.threadlog";
                _watcher.Created += new System.IO.FileSystemEventHandler(OnFileCreated);
                _watcher.EnableRaisingEvents = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogSnapshot LoadThreadLogFromFile(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
                return (ThreadLogSnapshot)serializer.ReadObject(file);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void OnLogFolderChanged(object sender, EventArgs e)
        {
            ResetFileWatcher();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void OnFileCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            for ( int retryCount = 0 ; retryCount < 5 ; retryCount++ )
            {
                try
                {
                    var threadLog = LoadThreadLogFromFile(e.FullPath);
                    Dispatcher.BeginInvoke(new Action(() => _viewModel.Logs.AddThreadLog(threadLog)));
                }
                catch ( IOException )
                {
                }

                Thread.Sleep(250);
            }
        }
 
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnIsWatchingChanged(object sender, EventArgs e)
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
    }
}
