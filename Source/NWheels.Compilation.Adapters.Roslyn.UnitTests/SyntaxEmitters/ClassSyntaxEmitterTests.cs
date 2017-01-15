using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class ClassSyntaxEmitterTests
    {
        [Fact]
        public void EmptyClass()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ClassWithBase()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.BaseType = new TypeMember(typeof(System.IO.Stream));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne : System.IO.Stream { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ClassWithInterface()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Interfaces.Add(new TypeMember(typeof(System.IDisposable)));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne : System.IDisposable { }"
            );
        }
    }
}
