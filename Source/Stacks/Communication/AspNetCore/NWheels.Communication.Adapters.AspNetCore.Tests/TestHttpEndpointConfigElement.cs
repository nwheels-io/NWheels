using System;
using System.Collections.Generic;
using NWheels.Communication.Api.Http;
using NWheels.Configuration.Api;
using NWheels.Testability;
using Xunit;

namespace NWheels.Communication.Adapters.AspNetCore.Tests
{
    public class TestHttpEndpointConfiguration : IHttpEndpointConfigElement
    {
        public TestHttpEndpointConfiguration()
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
            return new TestHttpsConfig();
        }

        private class HttpStaticFolderConfigList : List<IHttpStaticFolderConfig>, IConfigElementList<IHttpStaticFolderConfig>
        {
            public IHttpStaticFolderConfig NewItem()
            {
                return new TestHttpStaticFolderConfig();
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class TestHttpsConfig : IHttpsConfig
    {
        public int Port { get; set; }

        public bool RequireHttps { get; set; }

        public string CertFilePath { get; set; }

        public string CertFilePassword { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class TestHttpStaticFolderConfig : IHttpStaticFolderConfig
    {
        public string RequestBasePath { get; set; }

        public string LocalRootPath { get; set; }

        public IList<string> DefaultFiles { get; set; }

        public string CacheControl { get; set; }

        public string DefaultContentType { get; set; }

        public bool EnableDirectoryBrowsing { get; set; }
    }
}
