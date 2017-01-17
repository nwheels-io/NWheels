using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class RoslynTypeFactoryBackendTests
    {
        [Fact]
        public void CanCompileAssembly()
        {
            var backendUnderTest = new RoslynTypeFactoryBackend();

            var type1 = new TypeMember("RunTimeTypes", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            backendUnderTest.CompileSingleType(type1);

            
        }
    }
}
