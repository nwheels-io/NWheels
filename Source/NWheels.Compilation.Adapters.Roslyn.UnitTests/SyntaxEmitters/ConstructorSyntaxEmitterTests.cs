using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
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
    public class ConstructorSyntaxEmitterTests
    {
        public static IEnumerable<object[]> TestCases_CanEmitConstructorDeclaration = new object[][] {
            #region Test cases
            new object[] {
                "public ClassOne() { }",
                MemberVisibility.Public, MemberModifier.None, new Action(() => { }),
                null
            },
            new object[] {
                "public ClassOne(int num, string str) { }",
                MemberVisibility.Public, MemberModifier.None, new Action<int, string>((int num, string str) => { }),
                null
            },
            new object[] {
                "public ClassOne(ref int num, out string str) { }",
                MemberVisibility.Public, MemberModifier.None, new SignatureWithRefOutParams((ref int num, out string str) => { str = null; }),
                null
            },
            new object[] {
                "private ClassOne() { }",
                MemberVisibility.Private, MemberModifier.None, new Action(() => { }),
                null
            },
            new object[] {
                "protected ClassOne() { }",
                MemberVisibility.Protected, MemberModifier.None, new Action(() => { }),
                null
            },
            new object[] {
                "internal ClassOne() { }",
                MemberVisibility.Internal, MemberModifier.None, new Action(() => { }),
                null
            },
            new object[] {
                "internal protected ClassOne() { }",
                MemberVisibility.InternalProtected, MemberModifier.None, new Action(() => { }),
                null
            },
            new object[] {
                "static ClassOne() { }",
                MemberVisibility.Public, MemberModifier.Static, new Action(() => { }),
                null
            },
            new object[] {
                "public ClassOne(int x) : this(x, true) { }",
                MemberVisibility.Public, MemberModifier.None, new Action<int>((int x) => { }),
                new Action<ConstructorMember>(constructor => {
                    constructor.CallThisConstructor = new MethodCallExpression();
                    constructor.CallThisConstructor.Arguments.Add(new Argument {
                        Expression = new ParameterExpression {
                            Parameter = constructor.Signature.Parameters.First(p => p.Name == "x")
                        }
                    });
                    constructor.CallThisConstructor.Arguments.Add(new Argument {
                        Expression = new ConstantExpression { Value = true }
                    });
                })
            },
            new object[] {
                "public ClassOne(int x) : base(x, true) { }",
                MemberVisibility.Public, MemberModifier.None, new Action<int>((int x) => { }),
                new Action<ConstructorMember>(constructor => {
                    constructor.CallBaseConstructor = new MethodCallExpression();
                    constructor.CallBaseConstructor.Arguments.Add(new Argument {
                        Expression = new ParameterExpression {
                            Parameter = constructor.Signature.Parameters.First(p => p.Name == "x")
                        }
                    });
                    constructor.CallBaseConstructor.Arguments.Add(new Argument {
                        Expression = new ConstantExpression { Value = true }
                    });
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitConstructorDeclaration))]
        public void CanEmitConstructorDeclaration(
            string expectedCode,
            MemberVisibility visibility, 
            MemberModifier modifier,
            Delegate signaturePrototype,
            Action<ConstructorMember> constructorSetup)
        {
            //-- arrange

            var constructor = new ConstructorMember(visibility, modifier, "ClassOne", new MethodSignature(signaturePrototype.GetMethodInfo()));
            if (constructorSetup != null)
            {
                constructorSetup(constructor);
            }

            var enclosingClassMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            enclosingClassMember.Members.Add(constructor);

            var enclosingClassEmitter = new ClassSyntaxEmitter(enclosingClassMember);
            var expectedClassCode = "public class ClassOne { " + expectedCode + " }";

            //-- act

            var actualClassSyntax = enclosingClassEmitter.EmitSyntax();

            //-- assert

            actualClassSyntax.Should().BeEquivalentToCode(expectedClassCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void SignatureWithRefOutParams(ref int num, out string str);
    }
}
