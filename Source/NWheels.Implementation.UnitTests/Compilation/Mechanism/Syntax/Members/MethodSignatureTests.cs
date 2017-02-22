using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Syntax.Members
{
    public class MethodSignatureTests
    {
        public static readonly IEnumerable<object[]> TestCases_CanInitializeFromMethodInfo = new object[][] {
            #region Test cases
            new object[] {
                "void f()",
                new Action(() => { }),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();
                    sig.IsVoid.Should().BeTrue();
                    sig.ReturnValue.Should().BeNull();
                    sig.Parameters.Count.Should().Be(0);
                })
            },
            new object[] {
                "int f()",
                new Func<int>(() => 0),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();
                    sig.IsVoid.Should().BeFalse();
                    sig.ReturnValue.Should().NotBeNull();
                    sig.ReturnValue.Type.ClrBinding.Should().BeSameAs(typeof(int));
                    sig.Parameters.Count.Should().Be(0);
                })
            },
            new object[] {
                "void f(int num, string str)",
                new Action<int, string>((int num, string str) => { }),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();
                    sig.IsVoid.Should().BeTrue();
                    sig.ReturnValue.Should().BeNull();
                    sig.Parameters.Count.Should().Be(2);

                    sig.Parameters[0].Name.Should().Be("num");
                    sig.Parameters[0].Position.Should().Be(1);
                    sig.Parameters[0].Modifier.Should().Be(MethodParameterModifier.None);
                    sig.Parameters[0].Type.ClrBinding.Should().BeSameAs(typeof(int));

                    sig.Parameters[1].Name.Should().Be("str");
                    sig.Parameters[1].Position.Should().Be(2);
                    sig.Parameters[1].Modifier.Should().Be(MethodParameterModifier.None);
                    sig.Parameters[1].Type.ClrBinding.Should().BeSameAs(typeof(string));
                })
            },
            new object[] {
                "decimal f(int num, string str)",
                new Func<int, string, decimal>((int num, string str) => 0.5m),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();

                    sig.IsVoid.Should().BeFalse();
                    sig.ReturnValue.Should().NotBeNull();
                    sig.ReturnValue.Type.ClrBinding.Should().BeSameAs(typeof(decimal));

                    sig.Parameters.Count.Should().Be(2);

                    sig.Parameters[0].Name.Should().Be("num");
                    sig.Parameters[0].Position.Should().Be(1);
                    sig.Parameters[0].Modifier.Should().Be(MethodParameterModifier.None);
                    sig.Parameters[0].Type.ClrBinding.Should().BeSameAs(typeof(int));

                    sig.Parameters[1].Name.Should().Be("str");
                    sig.Parameters[1].Position.Should().Be(2);
                    sig.Parameters[1].Modifier.Should().Be(MethodParameterModifier.None);
                    sig.Parameters[1].Type.ClrBinding.Should().BeSameAs(typeof(string));
                })
            },
            new object[] {
                "void f(ref int num, out string str)",
                new VoidSignatureWithRefOutParams((ref int num, out string str) => { str = null; }),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();
                    sig.IsVoid.Should().BeTrue();
                    sig.ReturnValue.Should().BeNull();
                    sig.Parameters.Count.Should().Be(2);

                    sig.Parameters[0].Name.Should().Be("num");
                    sig.Parameters[0].Position.Should().Be(1);
                    sig.Parameters[0].Modifier.Should().Be(MethodParameterModifier.Ref);
                    sig.Parameters[0].Type.ClrBinding.Should().BeSameAs(typeof(int));

                    sig.Parameters[1].Name.Should().Be("str");
                    sig.Parameters[1].Position.Should().Be(2);
                    sig.Parameters[1].Modifier.Should().Be(MethodParameterModifier.Out);
                    sig.Parameters[1].Type.ClrBinding.Should().BeSameAs(typeof(string));
                })
            },
            new object[] {
                "bool f(ref int num, out string str)",
                new BoolSignatureWithRefOutParams((ref int num, out string str) => { str = null; return true; }),
                new Action<MethodSignature>(sig => {
                    sig.IsAsync.Should().BeFalse();

                    sig.IsVoid.Should().BeFalse();
                    sig.ReturnValue.Should().NotBeNull();
                    sig.ReturnValue.Type.ClrBinding.Should().BeSameAs(typeof(bool));

                    sig.Parameters.Count.Should().Be(2);

                    sig.Parameters[0].Name.Should().Be("num");
                    sig.Parameters[0].Position.Should().Be(1);
                    sig.Parameters[0].Modifier.Should().Be(MethodParameterModifier.Ref);
                    sig.Parameters[0].Type.ClrBinding.Should().BeSameAs(typeof(int));

                    sig.Parameters[1].Name.Should().Be("str");
                    sig.Parameters[1].Position.Should().Be(2);
                    sig.Parameters[1].Modifier.Should().Be(MethodParameterModifier.Out);
                    sig.Parameters[1].Type.ClrBinding.Should().BeSameAs(typeof(string));
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanInitializeFromMethodInfo))]
        public void CanInitializeFromMethodInfo(string label, Delegate prototype, Action<MethodSignature> assertion)
        {
            //-- arrange

            var methodInfo = prototype.GetMethodInfo();

            //-- act

            var signatureUnderTest = new MethodSignature(methodInfo);

            //-- assert

            assertion(signatureUnderTest);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void VoidSignatureWithRefOutParams(ref int num, out string str);
        public delegate bool BoolSignatureWithRefOutParams(ref int num, out string str);
    }
}
