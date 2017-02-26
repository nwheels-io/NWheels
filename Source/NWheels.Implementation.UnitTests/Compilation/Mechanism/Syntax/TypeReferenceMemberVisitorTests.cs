using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Syntax
{
    public class TypeReferenceMemberVisitorTests
    {
        [Fact]
        public void Visit_FieldTypesIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            type1.Members.Add(new FieldMember(type1, MemberVisibility.Private, MemberModifier.None, typeof(int), "_number"));
            type1.Members.Add(new FieldMember(type1, MemberVisibility.Private, MemberModifier.Static, typeof(string), "_text"));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(int), typeof(string)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_PropertyTypesIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.None, typeof(TimeSpan), "Duration"));
            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.Static, typeof(DateTime), "Timestamp"));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(TimeSpan), typeof(DateTime)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_MethodSignatureTypesIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            type1.Members.Add(new MethodMember(MemberVisibility.Public, MemberModifier.None, "M1", new MethodSignature(
                new[] { new MethodParameter("x", 1, typeof(int)), new MethodParameter("y", 2, typeof(string)) },
                returnValue: null,
                isAsync: false
            )));
            type1.Members.Add(new MethodMember(MemberVisibility.Public, MemberModifier.Static, "M2", new MethodSignature(
                new MethodParameter[0],
                returnValue: new MethodParameter(null, -1, typeof(TimeSpan)),
                isAsync: false
            )));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(int), typeof(string), typeof(TimeSpan)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_ConstructorSignatureTypesIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            type1.Members.Add(new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "ClassOne", new MethodSignature()));
            type1.Members.Add(new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "ClassOne", new MethodSignature(
                new[] { new MethodParameter("x", 1, typeof(int)), new MethodParameter("y", 2, typeof(string)) },
                returnValue: null,
                isAsync: false
            )));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(int), typeof(string)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_EventDelegateTypesInluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS2", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo");

            type1.Members.Add(new EventMember(
                MemberVisibility.Public, MemberModifier.None, typeof(Action<DateTime>), "E1"));
            type1.Members.Add(new EventMember(
                MemberVisibility.Public, MemberModifier.Static, ((TypeMember)typeof(Action<>)).MakeGenericType(type2), "E2"));


            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(Action<>), typeof(DateTime), type2
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_GenericTypeDefinitionsAndArgumentsIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS2", MemberVisibility.Public, TypeMemberKind.Class, "ClassTwo");
            var type3 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS3", MemberVisibility.Public, TypeMemberKind.Class, "ClassThree");

            type1.Members.Add(new PropertyMember(
                type1, MemberVisibility.Public, MemberModifier.None, ((TypeMember)typeof(IList<>)).MakeGenericType(type2), "Twos"));

            type1.Members.Add(new PropertyMember(
                type1, MemberVisibility.Public, MemberModifier.Static, ((TypeMember)typeof(IDictionary<,>)).MakeGenericType(type2, type3), "ThreeByTwo"));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(IList<>), typeof(IDictionary<,>), type2, type3
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_MethodBodyStatementsIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var method1 = new MethodMember(MemberVisibility.Public, MemberModifier.None, "M1", new MethodSignature());
            var variable1 = new LocalVariable { Name = "x", Type = typeof(TimeSpan) };

            method1.Body = new BlockStatement(
                new VariableDeclarationStatement { Variable = variable1 }
            );
                
            type1.Members.Add(method1);

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(TimeSpan)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_ConstructorBodyStatementsIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var constructor1 = new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "ClassOne", new MethodSignature());
            var variable1 = new LocalVariable { Name = "x", Type = typeof(TimeSpan) };

            constructor1.Body = new BlockStatement(
                new VariableDeclarationStatement { Variable = variable1 }
            );

            type1.Members.Add(constructor1);

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                type1, typeof(TimeSpan)
            });
        }
    }
}
