using System;
using System.Collections.Generic;
using NWheels.Communication.Api.Http;
using NWheels.Testability;
using Xunit;

namespace NWheels.Communication.Adapters.AspNetCore.Tests
{
    public class TestHttpEndpointConfiguration : IHttpEndpointConfigElement
    {
        public TestHttpEndpointConfiguration()
        {
            StaticFolders = new List<IHttpStaticFolderConfig>();
        }

        public int Port { get; set; }

        public IHttpsConfig Https { get; set; }

        public IList<IHttpStaticFolderConfig> StaticFolders { get; set; }

        public string Name { get; set; }
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
