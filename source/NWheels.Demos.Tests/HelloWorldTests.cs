using System.IO;
using MetaPrograms.Testability;
using NUnit.Framework;
using NWheels.Build;
using Shouldly;

namespace NWheels.Demos.Tests
{
    public class HelloWorldTests
    {
        [Test]
        public void TranspilesAsExpected()
        {
            var output = new TestCodeGeneratorOutput();
            var engine = new BuildEngine(new BuildOptions {
                ProjectFilePath = DemoProjectFilePath
            });

            var success = engine.Build(output);
            
            success.ShouldBeTrue();
            output.ShouldMatchFolder(DemoExpectedTranspilationDirectory);
        }

        private static string DemoRootDirectory =>
            TestContext.CurrentContext.GetDemoRootDirectory("01-hello-world");

        private static string DemoProjectFilePath =>
            Path.Combine(DemoRootDirectory, "dsl", "Demo.HelloWorld", "Demo.HelloWorld.csproj");

        private static string DemoExpectedTranspilationDirectory =>
            Path.Combine(DemoRootDirectory, "transpiled");
    }
}