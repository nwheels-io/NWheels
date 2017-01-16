using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
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
                "NS1.C1",
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1")
            },
            new object[] {
                "NS1.C1<NS2.C2>",
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1",
                    new TypeMember("NS2", MemberVisibility.Public, TypeMemberKind.Class, "C2")
                ),
            },
            new object[] {
                "NS1.C1<NS2.C2, NS3.C3<NS4.C4>>",
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1",
                    new TypeMember("NS2", MemberVisibility.Public, TypeMemberKind.Class, "C2"),
                    new TypeMember("NS3", MemberVisibility.Public, TypeMemberKind.Class, "C3",
                        new TypeMember("NS4", MemberVisibility.Public, TypeMemberKind.Class, "C4"))),
            },
            new object[] {
                "System.String",
                new TypeMember(typeof(string)),
            },
            new object[] {
                "System.Collections.Generic.List<System.String>",
                new TypeMember(typeof(List<string>)),
            },
            new object[] {
                "System.Collections.Generic.Dictionary<System.Int32, System.Collections.Generic.List<System.String>>",
                new TypeMember(typeof(Dictionary<int, List<string>>)),
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_TestGetTypeNameSyntax))]
        public void TestGetTypeNameSyntax(string expectedCode, TypeMember type)
        {
            //-- act

            var actualSyntax = SyntaxHelpers.GetTypeNameSyntax(type);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }
    }
}
