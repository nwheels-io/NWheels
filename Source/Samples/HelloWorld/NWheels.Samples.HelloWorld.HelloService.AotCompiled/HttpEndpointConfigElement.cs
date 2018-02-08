using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using NWheels.Communication.Api.Http;
using NWheels.Configuration.Api;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HttpEndpointConfigElement : IHttpEndpointConfigElement
    {
        public HttpEndpointConfigElement()
        {
            StaticFolders = new HttpStaticFolderConfigList();
            MiddlewarePipeline = new List<Type>();
        }

        public int Port { get; set; }

        public IHttpsConfig Https { get; set; }

        public IConfigElementList<IHttpStaticFolderConfig> StaticFolders { get; }

        public string Name { get; set; }

        public IList<Type> MiddlewarePipeline { get; set; }

        public IHttpsConfig NewHttpsConfig()
        {
            return new HttpsConfig();
        }

        private class HttpStaticFolderConfigList : List<IHttpStaticFolderConfig>, IConfigElementList<IHttpStaticFolderConfig>
        {
            public IHttpStaticFolderConfig NewItem()
            {
                return new HttpStaticFolderConfig();
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class HttpsConfig : IHttpsConfig
    {
        public int Port { get; set; }

        public bool RequireHttps { get; set; }

        public string CertFilePath { get; set; }

        public string CertFilePassword { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class HttpStaticFolderConfig : IHttpStaticFolderConfig
    {
        public string RequestBasePath { get; set; }

        public string LocalRootPath { get; set; }

        public IList<string> DefaultFiles { get; set; }

        public string CacheControl { get; set; }

        public string DefaultContentType { get; set; }

        public bool EnableDirectoryBrowsing { get; set; }
    }
}