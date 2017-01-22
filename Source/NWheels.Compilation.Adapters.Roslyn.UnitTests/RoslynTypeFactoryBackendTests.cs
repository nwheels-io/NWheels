using Microsoft.CodeAnalysis.CSharp;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class RoslynTypeFactoryBackendTests
    {
        [Fact]
        public void CanIncludeSourceLocationDirective()
        {
        }


        [Fact]
        public void CanCompileAssembly()
        {
            var backendUnderTest = new RoslynTypeFactoryBackend();

            var type1 = new TypeMember("RunTimeTypes", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            backendUnderTest.Compile(new[] { type1 });

            
        }
    }
}
