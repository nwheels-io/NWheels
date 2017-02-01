#if false

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
                MemberVisibility.Public, MethodParameter.ReturnVoid, "VoidMethod", new MethodParameter[0]
                new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "C1"),
                "NS1.C1"
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitMethodDeclaration))]
        public void CanEmitMethodDeclaration(
            string name,
            MemberVisibility visibility, 
            MethodInfo signature,
            string expectedCode)
        {
            //var key = new TypeKey(this.GetType(), typeof(string));
            //var type = new TypeMember(
            //    key.ToGeneratorInfo(), 
            //    "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            //-- arrange

            var method = new MethodMember(visibility, returnValue, name, parameters);
            var emitterUnderTest = new MethodSyntaxEmitter(method);
            
            //-- act

            var actualSyntax = emitterUnderTest.EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }


        private static MethodSignature Signature<TDelegate>(TDelegate prototype)
        {
            var methodInfo = ((Delegate)(object)prototype).GetMethodInfo();
            var signature = new MethodSignature();

            if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
            {
                signature.ReturnValue = new MethodParameter() {
                    Type = methodInfo.ReturnType
                };
            }

            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                signature.Parameters.Add(new MethodParameter(
                    parameterInfo.Name, 
                    position: signature.Parameters.Count + 1,
                    type: parameterInfo.ParameterType,
                    modifier: GetParameterModifier(parameterInfo),
                    attributes: GatParameterAttributes())
            }
        }
    }
}

#endif