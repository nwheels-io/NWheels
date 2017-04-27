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
        int Port { get; }
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
        string RequestBasePath { get; }
        string LocalRootPath { get; }
        IList<string> DefaultFiles { get; }
        string CacheControl { get; }
        string DefaultContentType { get; }
        bool EnableDirectoryBrowsing { get; }
    }
}
