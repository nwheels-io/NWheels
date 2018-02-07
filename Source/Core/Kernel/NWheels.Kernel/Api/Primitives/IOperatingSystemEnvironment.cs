using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NWheels.Kernel.Api.Primitives
{
    public interface IOperatingSystemEnvironment
    {
        bool IsOSPlatform(OSPlatform platform);
        string GetEnvironmentVariable(string variable);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RealOperatingSystemEnvironment : IOperatingSystemEnvironment
    {
        public bool IsOSPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetEnvironmentVariable(string variable)
        {
            return System.Environment.GetEnvironmentVariable(variable);
        }
    }
}
