using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [ConfigurationSection(XmlName = "Framework.Database")]
    public interface IFrameworkDatabaseConfig : IConfigurationSection
    {
        [PropertyContract.Security.Sensitive]
        string ConnectionString { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Security.Sensitive]
        string MasterConnectionString { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INamedObjectCollection<IFrameworkContextPersistenceConfig> Contexts { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement(XmlName = "Context")]
    public interface IFrameworkContextPersistenceConfig : INamedConfigurationElement
    {
        [PropertyContract.Semantic.InheritorOf(typeof(IApplicationDataRepository))]
        Type Contract { get; set; }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Security.Sensitive]
        string ConnectionString { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Security.Sensitive]
        string MasterConnectionString { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class FrameworkDatabaseConfigExtensions
    {
        public static string GetContextConnectionString(this IFrameworkDatabaseConfig configSection, Type domainContextType)
        {
            var contextSpecificConfig = configSection.Contexts.FirstOrDefault(ctx => ctx.Contract == domainContextType);

            if ( contextSpecificConfig != null )
            {
                return contextSpecificConfig.ConnectionString;
            }

            return configSection.ConnectionString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMasterConnectionString(this IFrameworkDatabaseConfig configSection, Type domainContextType)
        {
            var contextSpecificConfig = configSection.Contexts.FirstOrDefault(ctx => ctx.Contract == domainContextType);

            if ( contextSpecificConfig != null )
            {
                return contextSpecificConfig.MasterConnectionString;
            }

            return configSection.MasterConnectionString;
        }
    }
}
