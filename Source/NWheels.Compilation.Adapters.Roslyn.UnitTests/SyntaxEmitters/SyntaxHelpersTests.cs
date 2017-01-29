using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class SyntaxHelpersTests
    {
        public static IEnumerable<object[]> TestCases_TestGetTypeNameSyntax = new object[][] {
            #region Test cases
            new object[] {
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1"),
                "NS1.C1"
            },
            new object[] {
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1",
                    new TypeMember("NS2", MemberVisibility.Public, TypeMemberKind.Class, "C2")
                ),
                "NS1.C1<NS2.C2>"
            },
            new object[] {
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1",
                    new TypeMember("NS2", MemberVisibility.Public, TypeMemberKind.Class, "C2"),
                    new TypeMember("NS3", MemberVisibility.Public, TypeMemberKind.Class, "C3",
                        new TypeMember("NS4", MemberVisibility.Public, TypeMemberKind.Class, "C4"))),
                "NS1.C1<NS2.C2, NS3.C3<NS4.C4>>"
            },
            new object[] {
                new TypeMember(typeof(string)),
                "System.String"
            },
            new object[] {
                new TypeMember(typeof(List<string>)),
                "System.Collections.Generic.List<System.String>"
            },
            new object[] {
                new TypeMember(typeof(Dictionary<int, List<string>>)),
                "System.Collections.Generic.Dictionary<System.Int32, System.Collections.Generic.List<System.String>>"
            },
            new object[] {
                new TypeMember(typeof(TestNestedType)),
                "NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters.SyntaxHelpersTests.TestNestedType"
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_TestGetTypeNameSyntax))]
        public void TestGetTypeNameSyntax(TypeMember type, string expectedCode)
        {
            //-- act

            var actualSyntax = SyntaxHelpers.GetTypeNameSyntax(type);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_TestGetLiteralSyntax = new object[][] {
            #region Test cases
            new object[] { 123 , "123" },
            new object[] { 1234.56m, "1234.56M" },
            new object[] { "ABC" , "\"ABC\"" },
            new object[] { 'a' , "'a'" },
            new object[] { null, "null", },
            new object[] { typeof(string), "typeof(System.String)" },
            new object[] { new TypeMember(typeof(string)), "typeof(System.String)" },
            new object[] { typeof(Dictionary<int, string>), "typeof(System.Collections.Generic.Dictionary<System.Int32, System.String>)" },
            new object[] { new TypeMember(typeof(Dictionary<int, string>)), "typeof(System.Collections.Generic.Dictionary<System.Int32, System.String>)" },
            new object[] { new ConstantExpression() { Value = 123 }, "123" },
            new object[] { new ConstantExpression() { Value = null }, "null"  }
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_TestGetLiteralSyntax))]
        public void TestGetLiteralSyntax(object value, string expectedCode)
        {
            //-- act

            var actualSyntax = SyntaxHelpers.GetLiteralSyntax(value);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestNestedType {  }
    }
}
