using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NWheels.Utilities;

namespace NWheels.Core.Logging
{
    internal class ThreadRegistry : IThreadRegistry
    {
        private readonly object _syncRoot = new object();
        private readonly HashSet<ThreadLog> _runningThreads = new HashSet<ThreadLog>();
        private readonly string _logFolder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadRegistry()
            : this(PathUtility.LocalBinPath("Logs"))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadRegistry(string logFolder)
        {
            _logFolder = logFolder;

            if ( !Directory.Exists(_logFolder) )
            {
                Directory.CreateDirectory(_logFolder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ThreadStarted(ThreadLog threadLog)
        {
            lock ( _syncRoot )
            {
                _runningThreads.Add(threadLog);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ThreadFinished(ThreadLog threadLog)
        {
            lock ( _syncRoot )
            {
                _runningThreads.Remove(threadLog);
            }

            PersistLogInXmlFormat(threadLog);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLog[] GetRunningThreads()
        {
            lock ( _syncRoot )
            {
                return _runningThreads.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PersistLogInXmlFormat(ThreadLog log)
        {
            var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
            var fileName = log.TaskType.ToString() + "-" + log.LogId.ToString("N") + ".threadlog";

            using ( var file = File.Create(Path.Combine(_logFolder, fileName)) )
            {
                var writer = XmlWriter.Create(file);
                serializer.WriteObject(writer, log.TakeSnapshot());
                writer.Flush();
            }
        }
    }
}
