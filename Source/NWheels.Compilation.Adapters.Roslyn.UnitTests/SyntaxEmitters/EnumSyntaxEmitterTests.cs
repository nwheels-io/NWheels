using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class EnumSyntaxEmitterTests
    {
        [Fact]
        public void SimpleEnum()
        {
            //-- arrange

            var enumMember = new TypeMember();
            enumMember.Namespace = "SyntaxGeneratorTests";
            enumMember.Name = "TestEnum";
            enumMember.Members.Add(new EnumMember() { Name = "First" });
            enumMember.Members.Add(new EnumMember() { Name = "Second" });
            enumMember.Members.Add(new EnumMember() { Name = "Third" });

            var emitter = new EnumSyntaxEmitter(enumMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public enum TestEnum { First, Second, Third }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void FlagsEnum()
        {
            //-- arrange

            var enumMember = new TypeMember();
            enumMember.Namespace = "SyntaxGeneratorTests";
            enumMember.Name = "TestFlagsEnum";
            enumMember.Attributes.Add(new AttributeInfo() {
                AttributeType = new TypeMember(typeof(FlagsAttribute))
            });
            enumMember.Members.Add(new EnumMember() { Name = "None", Value = 0 });
            enumMember.Members.Add(new EnumMember() { Name = "First", Value = 0x01 });
            enumMember.Members.Add(new EnumMember() { Name = "Second", Value = 0x02 });
            enumMember.Members.Add(new EnumMember() { Name = "Both", Value = 0x01 | 0x02 });

            var emitter = new EnumSyntaxEmitter(enumMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "[System.FlagsAttribute] public enum TestFlagsEnum { None = 0, First = 1, Second = 2, Both = 3 }"
            );
        }
    }
}
