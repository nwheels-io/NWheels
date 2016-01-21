using Topshelf;
using Topshelf.Hosts;

namespace NWheels.Hosts.Console
{
    internal static class TopShelfExtensions
    {
        public static bool IsRunningAsConsole(this HostControl control)
        {
            return control is ConsoleRunHost;
        }
    }
}