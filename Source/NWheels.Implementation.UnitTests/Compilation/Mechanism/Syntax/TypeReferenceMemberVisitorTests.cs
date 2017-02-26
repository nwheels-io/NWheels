using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
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
            type1.Members.Add(new FieldMember(type1, MemberVisibility.Private, MemberModifier.None, typeof(string), "_text"));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                typeof(int), typeof(string)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Visit_PropertyTypesIncluded()
        {
            //-- arrange

            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType()), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.None, typeof(TimeSpan), "Duration"));
            type1.Members.Add(new PropertyMember(type1, MemberVisibility.Public, MemberModifier.None, typeof(DateTime), "Timestamp"));

            var foundTypes = new HashSet<TypeMember>();
            var visitorUnderTest = new TypeReferenceMemberVisitor(foundTypes);

            //-- act

            type1.AcceptVisitor(visitorUnderTest);

            //-- assert

            foundTypes.Should().BeEquivalentTo(new TypeMember[] {
                typeof(TimeSpan), typeof(DateTime)
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
            type1.Members.Add(new MethodMember(MemberVisibility.Public, MemberModifier.None, "M2", new MethodSignature(
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
                typeof(int), typeof(string), typeof(TimeSpan)
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
                typeof(int), typeof(string)
            });
        }
    }
}
