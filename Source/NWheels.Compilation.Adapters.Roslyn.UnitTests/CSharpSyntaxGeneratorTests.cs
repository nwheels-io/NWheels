using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class CSharpSyntaxGeneratorTests
    {
        [Fact]
        public void Namespaces_SingleTypeWithNamespace()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace", MemberVisibility.Public, TypeMemberKind.Class, "MyClass")
            };

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

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
        public void Namespaces_MultipleTypesWithSameNamespace()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo")
            };

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

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
        public void Namespaces_MultipleTypesWithDifferentNamespaces()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();
            var typesToCompile = new TypeMember[] {
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                new TypeMember("My.Namespace2", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo"),
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassA"),
                new TypeMember("My.Namespace1", MemberVisibility.Public, TypeMemberKind.Class, "ClassThree"),
                new TypeMember("My.Namespace2", MemberVisibility.Public, TypeMemberKind.Class, "ClassFour"),
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassB"),
            };

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(typesToCompile);

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Usings_ImportAllNamespaces()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo");

            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.None, typeof(DateTime), "Time"));
            type2.BaseType = type1;

            var allReferencedTypes = new[] {
                type1, type2, typeof(DateTime)
            };

            type1.SafeBackendTag().IsNamespaceImported = true;
            type2.SafeBackendTag().IsNamespaceImported = true;
            ((TypeMember)typeof(DateTime)).SafeBackendTag().IsNamespaceImported = true;

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(
                new[] { type1, type2 }, 
                allReferencedTypes);

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                using System;
                using My.First;
                using My.Second;

                namespace My.First
                {
                    public class ClassOne 
                    {
                        public DateTime Time { get; }
                    }
                }
                namespace My.Second
                {
                    public class ClassTwo : ClassOne { }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IContractOne {  }
        public interface IContractTwo { }
        public interface IContractThree { }
        public interface IContractFour { }
    }
}
