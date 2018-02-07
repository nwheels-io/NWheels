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
        IHttpsConfig NewHttpsConfig();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement]
    public interface IHttpsConfig
    {
        int Port { get; set; }
        bool RequireHttps { get; set; }
        string CertFilePath { get; set; }
        string CertFilePassword { get; set; }
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
