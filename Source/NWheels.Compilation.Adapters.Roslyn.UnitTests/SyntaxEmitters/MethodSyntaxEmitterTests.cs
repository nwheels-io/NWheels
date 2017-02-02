using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class MethodSyntaxEmitterTests
    {
        public static IEnumerable<object[]> TestCases_CanEmitMethodDeclaration = new object[][] {
            #region Test cases
            new object[] {
                "public void TestMethod() { }",
                MemberVisibility.Public, MemberModifier.None, new Action(() => { })
            },
            new object[] {
                "public string TestMethod() { }",
                MemberVisibility.Public, MemberModifier.None, new Func<string>(() => null)
            },
            new object[] {
                "public void TestMethod(int num, string str) { }",
                MemberVisibility.Public, MemberModifier.None, new Action<int, string>((int num, string str) => { })
            },
            new object[] {
                "public decimal TestMethod(int num, string str) { }",
                MemberVisibility.Public, MemberModifier.None, new Func<int, string, decimal>((int num, string str) => 0.5m)
            },
            new object[] {
                "public void TestMethod(ref int num, out string str) { }",
                MemberVisibility.Public, MemberModifier.None, new VoidSignatureWithRefOutParams((ref int num, out string str) => { str = null; })
            },
            new object[] {
                "public bool TestMethod(ref int num, out string str) { }",
                MemberVisibility.Public, MemberModifier.None, new BoolSignatureWithRefOutParams((ref int num, out string str) => { str = null; return true; })
            },
            new object[] {
                "private void TestMethod() { }",
                MemberVisibility.Private, MemberModifier.None, new Action(() => { })
            },
            new object[] {
                "protected void TestMethod() { }",
                MemberVisibility.Protected, MemberModifier.None, new Action(() => { })
            },
            new object[] {
                "internal void TestMethod() { }",
                MemberVisibility.Internal, MemberModifier.None, new Action(() => { })
            },
            new object[] {
                "internal protected void TestMethod() { }",
                MemberVisibility.InternalProtected, MemberModifier.None, new Action(() => { })
            },
            new object[] {
                "public abstract void TestMethod() { }",
                MemberVisibility.Public, MemberModifier.Abstract, new Action(() => { })
            },
            new object[] {
                "public virtual void TestMethod() { }",
                MemberVisibility.Public, MemberModifier.Virtual, new Action(() => { })
            },
            new object[] {
                "public override void TestMethod() { }",
                MemberVisibility.Public, MemberModifier.Override, new Action(() => { })
            },
            new object[] {
                "public static void TestMethod() { }",
                MemberVisibility.Public, MemberModifier.Static, new Action(() => { })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitMethodDeclaration))]
        public void CanEmitMethodDeclaration(
            string expectedCode,
            MemberVisibility visibility, 
            MemberModifier modifier,
            Delegate signaturePrototype)
        {
            //-- arrange

            var method = new MethodMember(signaturePrototype.GetMethodInfo());
            method.Name = "TestMethod";
            method.Visibility = visibility;
            method.Modifier = modifier;

            var emitterUnderTest = new MethodSyntaxEmitter(method);
            
            //-- act

            var actualSyntax = emitterUnderTest.EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void VoidSignatureWithRefOutParams(ref int num, out string str);
        public delegate bool BoolSignatureWithRefOutParams(ref int num, out string str);
    }
}
