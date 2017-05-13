using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    public class ConfigSection_MessagingPlatformConfiguration : IMessagingPlatformConfiguration
    {
        public ConfigSection_MessagingPlatformConfiguration()
        {
            this.Endpoints = new Dictionary<string, IEndpointConfig>();
        }

        public IHttpEndpointConfig NewHttpEndpointConfig()
        {
            return new ConfigElement_HttpEndpointConfig();
        }

        public IHttpsConfig NewHttpsConfig()
        {
            return new ConfigElement_HttpsConfig();
        }

        public IHttpStaticFolderConfig NewHttpStaticFolderConfig()
        {
            return new ConfigElement_HttpStaticFolderConfig();
        }

        public IDictionary<string, IEndpointConfig> Endpoints { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ConfigElement_IEndpointConfig : IEndpointConfig
    {
        public string Name { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ConfigElement_HttpEndpointConfig : ConfigElement_IEndpointConfig, IHttpEndpointConfig
    {
        public int Port { get; set; }
        public IHttpsConfig Https { get; set; }
        public IList<IHttpStaticFolderConfig> StaticFolders { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ConfigElement_HttpsConfig : IHttpsConfig
    {
        public int Port { get; set; }
        public bool RequireHttps { get; set; }
        public string CertFilePath { get; set; }
        public string CertFilePassword { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ConfigElement_HttpStaticFolderConfig : IHttpStaticFolderConfig
    {
        public string RequestBasePath { get; set; }
        public string LocalRootPath { get; set; }
        public IList<string> DefaultFiles { get; set; }
        public string CacheControl { get; set; }
        public string DefaultContentType { get; set; }
        public bool EnableDirectoryBrowsing { get; set; }
    }
}
