using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.DataObjects;
using NWheels.Logging;
using NWheels.Utilities;
using SmartFormat;

namespace NWheels.Processing.Messages.Impl
{
    public class LocalFileContentTemplateProvider : ContentTemplateProviderBase
    {
        private readonly ILogger _logger;
        private readonly string _templateFolderPath;
        private readonly ConcurrentDictionary<object, string> _templateByType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocalFileContentTemplateProvider(ILogger logger, IConfig config)
        {
            _logger = logger;
            _templateFolderPath = PathUtility.GetAbsolutePath(config.TemplateFolderPath, PathUtility.HostBinPath());
            _templateByType = new ConcurrentDictionary<object, string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IContentTemplateProvider

        public override string GetTemplate(object contentType)
        {
            return _templateByType.GetOrAdd(contentType, LoadContentTemplate);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string LoadContentTemplate(object contentType)
        {
            var filePath = Path.Combine(_templateFolderPath, contentType.ToString() + ".txt");
            _logger.LoadingTemplate(filePath);

            try
            {
                return File.ReadAllText(filePath);
            }
            catch ( Exception e )
            {
                _logger.FailedToLoadTemplate(filePath, error: e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogVerbose]
            void LoadingTemplate(string filePath);
            
            [LogError]
            void FailedToLoadTemplate(string filePath, Exception error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "LocalFileContentTemplateProvider")]
        public interface IConfig : IConfigurationSection
        {
            [PropertyContract.DefaultValue("ContentTemplates"), PropertyContract.Semantic.LocalFilePath]
            string TemplateFolderPath { get; set; }
        }
    }
}
