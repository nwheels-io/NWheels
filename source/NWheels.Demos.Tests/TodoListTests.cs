using MetaPrograms;
using NWheels.Build;
using MetaPrograms.Testability;
using NUnit.Framework;
using NWheels.Demos.Tests.Helpers;
using Shouldly;

namespace NWheels.Demos.Tests
{
    public class TodoListTests
    {
        [Test]
        public void TranspilesAsExpected()
        {
            var output = new TestCodeGeneratorOutput();
            var engine = new BuildEngine(new BuildOptions {
                ProjectFilePath = DemoProjectFile.FullPath
            });

            var success = engine.Build(output);
            
            success.ShouldBeTrue();
            
            output.ShouldMatchFolder(
                DemoTranspiledDirectory.FullPath, 
                GitRepoFilter.Create(DemoTranspiledDirectory)
            );
        }

        private static FilePath DemoRootDirectory =>
            TestContext.CurrentContext.GetDemoRootDirectory("02-todo-list");

        private static FilePath DemoProjectFile =>
            DemoRootDirectory.Append("dsl", "Demo.TodoList", "Demo.TodoList.csproj");

        private static FilePath DemoTranspiledDirectory =>
            DemoRootDirectory.Append("transpiled");
    }
}
