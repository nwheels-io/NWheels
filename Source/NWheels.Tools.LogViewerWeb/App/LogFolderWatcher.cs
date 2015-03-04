using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Tools.LogViewerWeb.App
{
    internal class LogFolderWatcher : IDisposable
    {
        private readonly string _folderPath;
        private readonly TimeSpan _captureExpiration;
        private readonly int _maxStoredCaptures;
        private readonly Queue<FileCapture> _captures;
        private readonly object _capturedFilesSyncRoot = new object();
        private readonly System.Threading.Timer _watcherUpdateTimer;
        private FileSystemWatcher _watcher;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogFolderWatcher(string folderPath, TimeSpan captureExpiration, int maxStoredCaptures)
        {
            _folderPath = folderPath;
            _captureExpiration = captureExpiration;
            _maxStoredCaptures = maxStoredCaptures;

            _captures = new Queue<FileCapture>();
            _watcherUpdateTimer = new Timer(OnUpdateWatcher, state: null, dueTime: TimeSpan.FromSeconds(1), period: TimeSpan.FromSeconds(3));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _watcherUpdateTimer.Dispose();

            var currentWatcher = _watcher;

            if ( currentWatcher != null )
            {
                currentWatcher.Dispose();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogSnapshot[] GetCapturedLogs(ref long lastCaptureId)
        {
            FileCapture[] currentCaptures;
            var newCaptures = new List<ThreadLogSnapshot>();

            lock ( _capturedFilesSyncRoot )
            {
                currentCaptures = _captures.ToArray();
            }

            foreach ( var capture in currentCaptures )
            {
                if ( capture.CaptureId > lastCaptureId )
                {
                    newCaptures.Add(capture.DeserializedContents);
                    lastCaptureId = capture.CaptureId;
                }
            }

            return newCaptures.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetLogById(Guid id, out ThreadLogSnapshot log)
        {
            var filePath = Path.Combine(_folderPath, id.ToString("N") + ".threadlog");

            if ( File.Exists(filePath) )
            {
                log = LoadFileContents(filePath);
                return true;
            }
            else
            {
                log = null;
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FolderPath
        {
            get { return _folderPath; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnUpdateWatcher(object state)
        {
            var folderExists = Directory.Exists(_folderPath);

            if ( folderExists && _watcher == null )
            {
                Console.WriteLine("> STARTING WATCHER ON: {0}", _folderPath);

                _watcher = new System.IO.FileSystemWatcher();
                _watcher.Path = _folderPath;
                _watcher.NotifyFilter = System.IO.NotifyFilters.FileName;
                _watcher.Filter = "*.threadlog";
                _watcher.Created += new System.IO.FileSystemEventHandler(OnFileCreated);
                _watcher.EnableRaisingEvents = true;
            }
            else if ( !folderExists && _watcher != null )
            {
                Console.WriteLine("> STOPPING WATCHER ON: {0}", _folderPath);

                _watcher.Dispose();
                _watcher = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnFileCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            if ( e.ChangeType == WatcherChangeTypes.Created )
            {
                lock ( _capturedFilesSyncRoot )
                {
                    var now = DateTime.Now;

                    _captures.Enqueue(new FileCapture(e.FullPath, now));
                    PurgeExpiredCaptures(now);
                }

                Console.WriteLine("> CAPTURED: {0}", e.FullPath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PurgeExpiredCaptures(DateTime now)
        {
            var minRetentionTime = now.Subtract(_captureExpiration);
            var purgedList = new List<FileCapture>();

            while ( _captures.Count > 1 && (_captures.Count > 100 || _captures.Peek().CapturedAt < minRetentionTime) )
            {
                purgedList.Add(_captures.Dequeue());
            }

            foreach ( var purged in purgedList )
            {
                Console.WriteLine("> purged: {0}", purged.Path);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ThreadLogSnapshot LoadFileContents(string filePath)
        {
            var retryCountDown = 5;

            while ( true )
            {
                try
                {
                    using ( var file = File.OpenRead(filePath) )
                    {
                        var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
                        return (ThreadLogSnapshot)serializer.ReadObject(file);
                    }
                }
                catch ( IOException )
                {
                    if ( --retryCountDown > 0 )
                    {
                        Thread.Sleep(250);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static long _s_lastFileCaptureId = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FileCapture
        {
            private ThreadLogSnapshot _deserializedContents = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FileCapture(string path, DateTime now)
            {
                this.CaptureId = Interlocked.Increment(ref _s_lastFileCaptureId);
                this.Path = path;
                this.CapturedAt = now;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public long CaptureId { get; private set; }
            public string Path { get; private set; }
            public DateTime CapturedAt { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogSnapshot DeserializedContents
            {
                get
                {
                    if ( _deserializedContents == null )
                    {
                        _deserializedContents = LoadFileContents(this.Path);
                    }

                    return _deserializedContents;
                }
            }
        }
    }
}
