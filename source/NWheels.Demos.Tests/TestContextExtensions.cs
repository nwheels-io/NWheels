using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MetaPrograms;
using NUnit.Framework;

namespace NWheels.Demos.Tests
{
    public static class TestContextExtensions
    {
        public static FilePath GetDemoRootDirectory(this TestContext context, string demoName)
        {
            return FilePath.Parse(context.TestDirectory)
                .Up(5)
                .Append("demos", demoName);
        }
    }
}
