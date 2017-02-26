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
    public class CSharpSyntaxGeneratorTests : SyntaxEmittingTestBase
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
        public void Usings_UniqueTypeNames_NamespacesImported()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo");

            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.None, typeof(DateTime), "Time"));
            type2.BaseType = type1;

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(
                typesToCompile: new[] { type1, type2 }, 
                allReferencedTypes: new[] { type1, (TypeMember)typeof(DateTime) });

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                using System;
                using My.First;

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

        [Fact]
        public void Usings_DuplicateTypeNames_TypesAreNamespaceQualified()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();

            var typeFirstClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeSecondClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeThirdClassC = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Third", MemberVisibility.Public, TypeMemberKind.Class, "ClassC");

            typeThirdClassC.Members.Add(new PropertyMember(typeThirdClassC, MemberVisibility.Public, MemberModifier.None, typeFirstClassA, "FirstA"));
            typeThirdClassC.Members.Add(new PropertyMember(typeThirdClassC, MemberVisibility.Public, MemberModifier.None, typeSecondClassA, "SecondA"));

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(
                typesToCompile: new[] { typeFirstClassA, typeSecondClassA, typeThirdClassC },
                allReferencedTypes: new[] { typeFirstClassA, typeSecondClassA });

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                namespace My.First
                {
                    public class ClassA 
                    {
                    }
                }
                namespace My.Second
                {
                    public class ClassA
                    {
                    }
                }
                namespace My.Third
                {
                    public class ClassC
                    {
                        public My.First.ClassA FirstA { get; }
                        public My.Second.ClassA SecondA { get; }
                    }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Usings_SomeTypeNamesDuplicate_OnlyDuplicatesNamespaceQualified()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();

            var typeFirstClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeFirstClassB = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassB");
            var typeSecondClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeSecondClassC = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassC");
            var typeThirdClassD = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Third", MemberVisibility.Public, TypeMemberKind.Class, "ClassD");

            typeThirdClassD.Members.Add(new PropertyMember(typeThirdClassD, MemberVisibility.Public, MemberModifier.None, typeFirstClassA, "FirstA"));
            typeThirdClassD.Members.Add(new PropertyMember(typeThirdClassD, MemberVisibility.Public, MemberModifier.None, typeFirstClassB, "FirstB"));
            typeThirdClassD.Members.Add(new PropertyMember(typeThirdClassD, MemberVisibility.Public, MemberModifier.None, typeSecondClassA, "SecondA"));
            typeThirdClassD.Members.Add(new PropertyMember(typeThirdClassD, MemberVisibility.Public, MemberModifier.None, typeSecondClassC, "SecondC"));

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(
                typesToCompile: new[] { typeFirstClassA, typeFirstClassB, typeSecondClassA, typeSecondClassC, typeThirdClassD },
                allReferencedTypes: new[] { typeFirstClassA, typeFirstClassB, typeSecondClassA, typeSecondClassC });

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                using My.First;
                using My.Second;

                namespace My.First
                {
                    public class ClassA 
                    {
                    }
                    public class ClassB
                    {
                    }
                }
                namespace My.Second
                {
                    public class ClassA
                    {
                    }
                    public class ClassC
                    {
                    }
                }
                namespace My.Third
                {
                    public class ClassD
                    {
                        public My.First.ClassA FirstA { get; }
                        public ClassB FirstB { get; }
                        public My.Second.ClassA SecondA { get; }
                        public ClassC SecondC { get; }
                    }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Usings_GenericTypeNames_RulesApplyRecursively()
        {
            //-- arrange

            var generatorUnderTest = new CSharpSyntaxGenerator();

            var typeFirstClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeFirstClassList = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.First", MemberVisibility.Public, TypeMemberKind.Class, "List",
                new TypeMember(new TypeGeneratorInfo(this.GetType()), null, MemberVisibility.Public, TypeMemberKind.GenericParameter, "T"));
            var typeSecondClassA = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassA");
            var typeSecondClassB = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Second", MemberVisibility.Public, TypeMemberKind.Class, "ClassB");
            var typeThirdClassC = new TypeMember(new TypeGeneratorInfo(this.GetType()), "My.Third", MemberVisibility.Public, TypeMemberKind.Class, "ClassC");

            TypeMember typeGenericDictionary = typeof(Dictionary<,>);
            TypeMember typeGenericList = typeof(List<>);

            var typeDictionaryOfFirstClassASecondClassA = typeGenericDictionary.MakeGenericType(typeFirstClassA, typeSecondClassA);
            var typeListOfSecondClassB = typeGenericList.MakeGenericType(typeSecondClassB);

            typeThirdClassC.Members.Add(new PropertyMember(
                typeThirdClassC, 
                MemberVisibility.Public, 
                MemberModifier.None,
                typeDictionaryOfFirstClassASecondClassA, 
                "SecondByFirst"));

            typeThirdClassC.Members.Add(new PropertyMember(
                typeThirdClassC,
                MemberVisibility.Public,
                MemberModifier.None,
                typeListOfSecondClassB,
                "ListOfB"));

            //-- act

            SyntaxTree syntax = generatorUnderTest.GenerateSyntax(
                typesToCompile: new[] {
                    typeFirstClassA, typeFirstClassList, typeSecondClassA, typeSecondClassB, typeThirdClassC
                },
                allReferencedTypes: new[] {
                    typeFirstClassA, typeFirstClassList, typeSecondClassA, typeSecondClassB, typeDictionaryOfFirstClassASecondClassA, typeListOfSecondClassB
                });

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                using System.Collections.Generic;
                using My.Second;

                namespace My.First
                {
                    public class ClassA 
                    {
                    }
                    public class List<T>
                    {
                    }
                }
                namespace My.Second
                {
                    public class ClassA
                    {
                    }
                    public class ClassB
                    {
                    }
                }
                namespace My.Third
                {
                    public class ClassC
                    {
                        public Dictionary<My.First.ClassA, My.Second.ClassA> SecondByFirst { get; }
                        public System.Collections.Generic.List<ClassB> ListOfB { get; }
                    }
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
