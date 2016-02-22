using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;

namespace NWheels.Configuration.Core
{
    public abstract class ConfigurationSourceBase : IConfigurationSource
    {
        protected ConfigurationSourceBase(string sourceType, ConfigurationSourceLevel sourceLevel)
        {
            this.SourceType = sourceType;
            this.SourceLevel = sourceLevel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IConfigurationSource

        public virtual IEnumerable<ConfigurationDocument> GetConfigurationDocuments()
        {
            return new ConfigurationDocument[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ApplyConfigurationProgrammatically(IComponentContext components)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SourceType { get; private set; }
        public ConfigurationSourceLevel SourceLevel { get; private set; }

        #endregion
    }
}
