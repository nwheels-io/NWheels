using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    //TODO: [ConfigurationSection]
    public interface IEndpointConfiguration
    {
        string Name { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    //TODO: [ConfigurationSection]
    public interface IHttpEndpointConfiguration : IEndpointConfiguration
    {
        int Port { get; }
        IHttpsConfig Https { get; set; }
        IList<IHttpStaticFolderConfig> StaticFolders { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    //TODO: [ConfigurationElement]
    public interface IHttpsConfig
    {
        int Port { get; }
        bool RequireHttps { get; }
        string CertFilePath { get; }
        string CertFilePassword { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    //TODO: [ConfigurationElement]
    public interface IHttpStaticFolderConfig
    {
        string RequestBasePath { get; }
        string LocalRootPath { get; }
        IList<string> DefaultDocuments { get; }
        string CacheControl { get; }
        string DefaultContentType { get; }
        bool EnableDirectoryBrowsing { get; }
    }
}
