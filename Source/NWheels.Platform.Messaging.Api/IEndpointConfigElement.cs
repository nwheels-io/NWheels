using NWheels.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    [ConfigElement]
    public interface IEndpointConfig
    {
        [ConfigElementKey]
        string Name { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IHttpEndpointConfig : IEndpointConfig
    {
        int Port { get; set; }
        IHttpsConfig Https { get; set; }
        IList<IHttpStaticFolderConfig> StaticFolders { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigElement]
    public interface IHttpsConfig
    {
        int Port { get; }
        bool RequireHttps { get; }
        string CertFilePath { get; }
        string CertFilePassword { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigElement]
    public interface IHttpStaticFolderConfig
    {
        string RequestBasePath { get; set; }
        string LocalRootPath { get; set; }
        IList<string> DefaultFiles { get; }
        string CacheControl { get; set; }
        string DefaultContentType { get; set; }
        bool EnableDirectoryBrowsing { get; set; }
    }
}
