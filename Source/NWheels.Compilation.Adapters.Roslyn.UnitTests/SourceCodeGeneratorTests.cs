using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class SourceCodeGeneratorTests
    {
        [Fact]
        public void SingleTypeWithNamespace()
        {
            //-- arrange

            var generatorUnderTest = new SourceCodeGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace", MemberVisibility.Public, TypeMemberKind.Class, "MyClass")
            };

            //-- act

            CompilationUnitSyntax syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                namespace My.Namespace
                {
                    public class MyClass { }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleTypesWithSameNamespace()
        {
            //-- arrange

            var generatorUnderTest = new SourceCodeGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo")
            };

            //-- act

            CompilationUnitSyntax syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                namespace My.Namespace1
                {
                    public class ClassOne { }
                    public class ClassTwo { }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MultipleTypesWithDifferentNamespaces()
        {
            //-- arrange

            var generatorUnderTest = new SourceCodeGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                new TypeMember("My.Namespace2", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo"),
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassA"),
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassThree"),
                new TypeMember("My.Namespace2", MemberVisibility.Public, TypeMemberKind.Class, "ClassFour"),
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassB"),
            };

            //-- act

            CompilationUnitSyntax syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassA { }
                public class ClassB { }
                namespace My.Namespace1
                {
                    public class ClassOne { }
                    public class ClassThree { }
                }
                namespace My.Namespace2
                {
                    public class ClassTwo { }
                    public class ClassFour { }
                }
            ");
        }
    }
}
