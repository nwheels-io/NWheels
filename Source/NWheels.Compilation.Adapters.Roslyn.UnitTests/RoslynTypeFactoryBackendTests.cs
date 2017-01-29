using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class RoslynTypeFactoryBackendTests
    {
        [Fact]
        public void CanCompileAssembly()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());

            var key1 = new RealTypeKey<Empty.KeyExtension>(this.GetType(), typeof(int), null, null);
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            type1.Namespace = "NS1";
            type1.Visibility = MemberVisibility.Public;
            type1.TypeKind = TypeMemberKind.Class;
            type1.Name = "ClassOne";

            //-- act

            var result = backendUnderTest.Compile(new[] { type1 });

            //-- assert

            result.Success.Should().BeTrue();
            result.Succeeded.Count.Should().Be(1);
            result.Succeeded[0].Type.Should().BeSameAs(type1);
            result.Failed.Count.Should().Be(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanReportCompilationIssuesPerTypeMember()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());

            var type1 = new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember("NS1", MemberVisibility.Private, TypeMemberKind.Class, "ClassTwo");
            var type3 = new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassThree");
            var type4 = new TypeMember("NS1", MemberVisibility.Private, TypeMemberKind.Class, "ClassFour");

            //-- act

            var result = backendUnderTest.Compile(new[] { type1, type2, type3, type4 });

            //-- assert

            result.Success.Should().BeFalse();
            result.Succeeded.Count.Should().Be(0);

            result.Failed.Count.Should().Be(4);

            result.Failed[0].Success.Should().BeTrue();
            result.Failed[0].Diagnostics.Count.Should().Be(0);

            result.Failed[1].Success.Should().BeFalse();
            result.Failed[1].Diagnostics.Count.Should().Be(1);
            result.Failed[1].Diagnostics[0].Severity.Should().Be(CompilationDiagnosticSeverity.Error);
            result.Failed[1].Diagnostics[0].Code.Should().Be("CS1527");
            result.Failed[1].Diagnostics[0].Message.Should().NotBeNullOrEmpty();
            result.Failed[1].Diagnostics[0].SourceLocation.Should().NotBeNullOrEmpty();

            result.Failed[2].Success.Should().BeTrue();
            result.Failed[2].Diagnostics.Count.Should().Be(0);

            result.Failed[3].Success.Should().BeFalse();
            result.Failed[3].Diagnostics.Count.Should().Be(1);
            result.Failed[3].Diagnostics[0].Severity.Should().Be(CompilationDiagnosticSeverity.Error);
            result.Failed[3].Diagnostics[0].Code.Should().Be("CS1527");
            result.Failed[3].Diagnostics[0].Message.Should().NotBeNullOrEmpty();
            result.Failed[3].Diagnostics[0].SourceLocation.Should().NotBeNullOrEmpty();
        }
    }
}
