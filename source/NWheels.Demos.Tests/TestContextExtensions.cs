using System.IO;
using NUnit.Framework;

namespace NWheels.Demos.Tests
{
    public static class TestContextExtensions
    {
        public static string GetDemoRootDirectory(this TestContext context, string demoName)
        {
            return Path.Combine(
                context.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "demos",
                demoName,
                "dsl"
            );
        }
    }
}
