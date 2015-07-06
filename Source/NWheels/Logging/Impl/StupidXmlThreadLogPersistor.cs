using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using NWheels.Hosting;
using NWheels.Logging.Core;
using NWheels.Utilities;

namespace NWheels.Logging.Impl
{
    internal class StupidXmlThreadLogPersistor : LifecycleEventListenerBase, IThreadLogPersistor
    {
        private readonly IComponentContext _components;
        private readonly IPlainLog _plainLog;
        private string _threadLogFolder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StupidXmlThreadLogPersistor(IComponentContext components, IPlainLog plainLog)
        {
            _components = components;
            _plainLog = plainLog;
            _threadLogFolder = PathUtility.HostBinPath("..\\BootLog");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Persist(IReadOnlyThreadLog threadLog)
        {
            if ( _threadLogFolder != null )
            {
                PersistLogInXmlFormat(threadLog);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured()
        {
            var configuration = _components.Resolve<IFrameworkLoggingConfiguration>();
            SetThreadLogFolder(configuration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetThreadLogFolder(IFrameworkLoggingConfiguration configuration)
        {
            _threadLogFolder = PathUtility.HostBinPath(configuration.ThreadLogFolder);

            if ( !Directory.Exists(_threadLogFolder) )
            {
                Directory.CreateDirectory(_threadLogFolder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PersistLogInXmlFormat(IReadOnlyThreadLog threadLog)
        {
            var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
            var fileName = threadLog.LogId.ToString("N") + ".threadlog";

            try
            {
                using ( var file = File.Create(Path.Combine(_threadLogFolder, fileName)) )
                {
                    var writer = XmlWriter.Create(file);
                    serializer.WriteObject(writer, threadLog.TakeSnapshot());
                    writer.Flush();
                }
            }
            catch ( Exception e )
            {
                _plainLog.Warning(e.Message);
            }
        }
    }
}
