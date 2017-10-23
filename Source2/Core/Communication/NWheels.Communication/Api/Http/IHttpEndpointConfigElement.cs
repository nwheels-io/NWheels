using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Configuration.Api;

namespace NWheels.Communication.Api.Http
{
    [ConfigurationElement]
    public interface IHttpEndpointConfigElement : IEndpointConfigElement
    {
        int Port { get; set; }
        IHttpsConfig Https { get; set; }
        IConfigElementList<IHttpStaticFolderConfig> StaticFolders { get; }
        IList<Type> MiddlewarePipeline { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement]
    public interface IHttpsConfig
    {
        int Port { get; }
        bool RequireHttps { get; }
        string CertFilePath { get; }
        string CertFilePassword { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement]
    public interface IHttpStaticFolderConfig
    {
        string RequestBasePath { get; set; }
        string LocalRootPath { get; set; }
        IList<string> DefaultFiles { get; set; }
        string CacheControl { get; set; }
        string DefaultContentType { get; set; }
        bool EnableDirectoryBrowsing { get; set; }
    }
}
