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
        public void TypeKeyAttribute_FullKey()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();

            var key1 = new TypeKey(
                factoryType: typeof(CSharpSyntaxGeneratorTests),
                primaryContract: typeof(IContractOne),
                secondaryContract1: typeof(IContractTwo),
                secondaryContract2: typeof(IContractThree),
                secondaryContract3: typeof(IContractFour),
                extensionValue1: 111,
                extensionValue2: 222,
                extensionValue3: 333);

            var type1 = new TypeMember(
                new TypeGeneratorInfo(this.GetType(), key1),
                "My.Namespace", MemberVisibility.Public, TypeMemberKind.Class, "MyClass");
            
            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(new[] { type1 });

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                namespace My.Namespace
                {
                    [NWheels.Compilation.Mechanism.Factories.TypeKeyAttribute(
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.CSharpSyntaxGeneratorTests),
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.CSharpSyntaxGeneratorTests.IContractOne),
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.CSharpSyntaxGeneratorTests.IContractTwo),
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.CSharpSyntaxGeneratorTests.IContractThree),
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.CSharpSyntaxGeneratorTests.IContractFour),
                        111, 222, 333)]
                    public class MyClass { }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SingleTypeWithNamespace()
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
        public void MultipleTypesWithSameNamespace()
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
        public void MultipleTypesWithDifferentNamespaces()
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

        public interface IContractOne {  }
        public interface IContractTwo { }
        public interface IContractThree { }
        public interface IContractFour { }
    }
}
