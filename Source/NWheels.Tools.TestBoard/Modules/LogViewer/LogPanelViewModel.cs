using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    public class LogPanelViewModel : INotifyPropertyChanged
    {
        private readonly List<ThreadLogModelTuple> _threadLogModelTuples;
        private ConcurrentQueue<ThreadLogSnapshot> _pendingThreadLogs;
        private CancellationTokenSource _cancellation;
        private List<Task> _loaderTasks;
        private TreeNodeItem<ThreadLogViewModel.NodeItem> _selectedItem;
        private ThreadLogViewModel.NodeItem _selectedNode;

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public LogPanelViewModel()
        {
            _threadLogModelTuples = new List<ThreadLogModelTuple>();
            _pendingThreadLogs = new ConcurrentQueue<ThreadLogSnapshot>();
            _cancellation = new CancellationTokenSource();
            _loaderTasks = new List<Task>();

            Items = new ObservableCollection<TreeNodeItem<ThreadLogViewModel.NodeItem>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void AddLogFromFile(string filePath)
        {
            Exception lastException = null;

            for ( int retryCount = 0; retryCount < 5; retryCount++ )
            {
                if ( retryCount > 0 )
                {
                    Thread.Sleep(200);
                }

                try
                {
                    var threadLog = TryLoadThreadLogFromFileOne(filePath);
                    _pendingThreadLogs.Enqueue(threadLog);
                    return;
                }
                catch ( IOException e )
                {
                    lastException = e;
                }
                catch ( Exception e )
                {
                    lastException = e;
                    break;
                }
            }

            AddFileLoadFailureThreadLog(filePath, lastException);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLog(ThreadLogSnapshot threadLog)
        {
            _pendingThreadLogs.Enqueue(threadLog);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLogsFromFolder(string folderPath)
        {
            _loaderTasks.Add(Task.Run(() => LoadAllLogsFromFolder(folderPath, _cancellation.Token)));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            if ( _cancellation != null )
            {
                _cancellation.Cancel();
            }

            foreach ( var task in _loaderTasks )
            {
                task.Wait(TimeSpan.FromSeconds(3));
            }

            _pendingThreadLogs = new ConcurrentQueue<ThreadLogSnapshot>();
            _threadLogModelTuples.Clear();
            _loaderTasks.Clear();
            _cancellation = new CancellationTokenSource();
            
            Items.Clear();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void DisplayPendingLogs()
        {
            int count = 0;
            ThreadLogSnapshot threadLog;

            TreeNodeItem<ThreadLogViewModel.NodeItem> rootCauseErrorNode = null;

            while ( _pendingThreadLogs.TryDequeue(out threadLog) && count++ < 100 )
            {
                var tuple = new ThreadLogModelTuple(this, threadLog);
                _threadLogModelTuples.Add(tuple);
                Items.Add(tuple.RootNodeItem);

                if ( rootCauseErrorNode == null )
                {
                    rootCauseErrorNode = tuple.RootNodeItem.ExpandToRootCauseError();

                    if ( rootCauseErrorNode != null )
                    {
                        this.SelectedItem = rootCauseErrorNode;
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public ObservableCollection<TreeNodeItem<ThreadLogViewModel.NodeItem>> Items { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TreeNodeItem<ThreadLogViewModel.NodeItem> SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                if ( PropertyChanged != null )
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogViewModel.NodeItem SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                _selectedNode = value;

                if ( PropertyChanged != null )
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedNode"));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadAllLogsFromFolder(string folderPath, CancellationToken cancel)
        {
            var fileList = Directory.GetFiles(folderPath, "*.threadLog", SearchOption.TopDirectoryOnly);

            foreach ( var filePath in fileList )
            {
                if ( cancel.IsCancellationRequested )
                {
                    break;
                }

                AddLogFromFile(filePath);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddFileLoadFailureThreadLog(string filePath, Exception exception)
        {
            _pendingThreadLogs.Enqueue(new ThreadLogSnapshot {
                StartedAtUtc = DateTime.UtcNow,
                TaskType = ThreadTaskType.LogProcessing,
                RootActivity = new ThreadLogSnapshot.LogNodeSnapshot() {
                    MessageId = "FailedToLoadFile",
                    IsActivity = true,
                    ContentTypes = LogContentTypes.Text | LogContentTypes.Exception,
                    ExceptionTypeName = exception.GetType().Name,
                    Level = LogLevel.Error,
                    NameValuePairs = new ThreadLogSnapshot.NameValuePairSnapshot[] {
                        new ThreadLogSnapshot.NameValuePairSnapshot { Name = "filePath", Value = filePath }
                    }
                }
            });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private static ThreadLogSnapshot TryLoadThreadLogFromFileOne(string filePath)
        {
            using ( var file = File.OpenRead(filePath) )
            {
                var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
                return (ThreadLogSnapshot)serializer.ReadObject(file);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadLogModelTuple
        {
            public ThreadLogModelTuple(LogPanelViewModel ownerModel, ThreadLogSnapshot threadLog)
            {
                this.ThreadLog = threadLog;
                this.ViewModel = new ThreadLogViewModel(threadLog);

                var lastTuple = ownerModel._threadLogModelTuples.LastOrDefault();

                this.RootNodeItem = new TreeNodeItem<ThreadLogViewModel.NodeItem>(
                    ViewModel.RootActivity, 
                    ownerModel.Items, 
                    parentNode: null, 
                    prevSiblingNode: (lastTuple != null ? lastTuple.RootNodeItem : null));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogSnapshot ThreadLog { get; private set; }
            public ThreadLogViewModel ViewModel { get; private set; }
            public TreeNodeItem<ThreadLogViewModel.NodeItem> RootNodeItem { get; private set; }
        }
    }
}
