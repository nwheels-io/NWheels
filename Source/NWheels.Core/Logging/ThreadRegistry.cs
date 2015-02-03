using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using NWheels.Core.Hosting;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Utilities;

namespace NWheels.Core.Logging
{
    internal class ThreadRegistry : IThreadRegistry, IInitializableHostComponent
    {
        private readonly object _syncRoot = new object();
        private readonly HashSet<ThreadLog> _runningThreads = new HashSet<ThreadLog>();
        private readonly IComponentContext _components;
        private IFrameworkLoggingConfiguration _loggingConfig;
        private string _threadLogFolder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadRegistry(IComponentContext components)
        {
            _components = components;
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

        void IInitializableHostComponent.Initializing()
        {
            _loggingConfig = _components.ResolveAuto<IFrameworkLoggingConfiguration>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IInitializableHostComponent.Configured()
        {
            _threadLogFolder = PathUtility.LocalBinPath(_loggingConfig.ThreadLogFolder);

            if ( !Directory.Exists(_threadLogFolder) )
            {
                Directory.CreateDirectory(_threadLogFolder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PersistLogInXmlFormat(ThreadLog log)
        {
            var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
            var fileName = log.TaskType.ToString() + "-" + log.LogId.ToString("N") + ".threadlog";

            using ( var file = File.Create(Path.Combine(_threadLogFolder, fileName)) )
            {
                var writer = XmlWriter.Create(file);
                serializer.WriteObject(writer, log.TakeSnapshot());
                writer.Flush();
            }
        }
    }
}
