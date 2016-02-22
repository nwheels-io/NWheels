using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWheels.Configuration.Core;
using NWheels.Hosting;
using NWheels.Hosting.Core;

namespace NWheels.Configuration.Impls
{
    public class XmlFileConfigurationSource : ConfigurationSourceBase
    {
        private readonly NodeHost _nodeHost;
        private readonly IConfigurationLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public XmlFileConfigurationSource(NodeHost nodeHost, IConfigurationLogger logger)
            : base("File", ConfigurationSourceLevel.Deployment)
        {
            _nodeHost = nodeHost;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConfigurationSourceBase

        public override IEnumerable<ConfigurationDocument> GetConfigurationDocuments()
        {
            foreach (var file in _nodeHost.BootConfig.ConfigFiles)
            {
                var document = LoadConfigurationDocument(file);

                if (document != null)
                {
                    yield return document;
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ConfigurationDocument LoadConfigurationDocument(BootConfiguration.ConfigFile file)
        {
            if (file.IsOptionalAndMissing)
            {
                _logger.OptionalFileNotPresentSkipping(file.Path);
                return null;
            }

            using (_logger.LoadingConfigurationFile(file.Path))
            {
                try
                {
                    var sourceInfo = new ConfigurationSourceInfo(this.SourceLevel, this.SourceType, file.Path);
                    var xml = XDocument.Load(file.Path, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

                    return new ConfigurationDocument(xml, sourceInfo);
                }
                catch (Exception e)
                {
                    _logger.FailedToLoadConfigurationFile(file.Path, e);
                    throw;
                }
            }
        }
    }
}
