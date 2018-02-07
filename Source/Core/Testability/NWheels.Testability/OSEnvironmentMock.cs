using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Primitives;

namespace NWheels.Testability
{
    public sealed class OSEnvironmentMock : IOperatingSystemEnvironment
    {
        public OSEnvironmentMock()
        {
            this.Environment = new Dictionary<string, string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OSEnvironmentMock(OSPlatform platform, IDictionary<string, string> environment)
        {
            this.Platform = platform;
            this.Environment = new Dictionary<string, string>(environment);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IOperatingSystemEnvironment.IsOSPlatform(OSPlatform platform)
        {
            return (this.Platform == platform);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IOperatingSystemEnvironment.GetEnvironmentVariable(string variable)
        {
            return this.Environment.GetValueOrDefault(variable, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OSPlatform Platform { get; set; }
        public IDictionary<string, string> Environment { get; }
    }
}
