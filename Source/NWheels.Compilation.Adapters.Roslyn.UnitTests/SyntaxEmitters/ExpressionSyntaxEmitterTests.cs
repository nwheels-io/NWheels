using Microsoft.CodeAnalysis.CSharp.Syntax;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class ExpressionSyntaxEmitterTests
    {
        public static IEnumerable<object[]> TestCases_CanEmitNewObjectExpression = new object[][] {
            #region Test cases
            new object[] {
                "new System.DateTime()",
                new Func<NewObjectExpression>(() => new NewObjectExpression {
                    Type = typeof(DateTime)
                })
            },
            new object[] {
                "new System.DateTime(2016, 12, 5)",
                new Func<NewObjectExpression>(() => {
                    var x = new NewObjectExpression() {
                        Type = typeof(DateTime)
                    };
                    x.ConstructorCall = new MethodCallExpression();
                    x.ConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression() { Value = 2016 } });
                    x.ConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression() { Value = 12 } });
                    x.ConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression() { Value = 5 } });
                    return x;
                })
            },
            new object[] {
                "new MyNS.MyClass<int, string>(typeof(object), new System.Uri(), ref num, out str)",
                new Func<NewObjectExpression>(() => {
                    var x = new NewObjectExpression() {
                        Type = new TypeMember("MyNS", MemberVisibility.Public, TypeMemberKind.Class, "MyClass", typeof(int), typeof(string))
                    };
                    x.ConstructorCall = new MethodCallExpression();
                    x.ConstructorCall.Arguments.Add(new Argument {
                        Expression = new ConstantExpression() { Value = typeof(object) }
                    });
                    x.ConstructorCall.Arguments.Add(new Argument {
                        Expression = new NewObjectExpression() { Type = typeof(System.Uri) }
                    });
                    x.ConstructorCall.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "num"} },
                        Modifier = MethodParameterModifier.Ref
                    });
                    x.ConstructorCall.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "str"} },
                        Modifier = MethodParameterModifier.Out
                    });
                    return x;
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitNewObjectExpression))]
        public void CanEmitNewObjectExpression(string expectedCode, Func<NewObjectExpression> expressionFactory)
        {
            //-- arraange

            var expression = expressionFactory();

            //-- act

            var actualSyntax = ExpressionSyntaxEmitter.EmitSyntax(expression);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanEmitMethodCallExpression = new object[][] {
            #region Test cases
            new object[] {
                "TestMethod()",
                new Func<MethodCallExpression>(() => new MethodCallExpression { MethodName = "TestMethod" })
            },
            new object[] {
                "TestMethod(123, new System.IO.MemoryStream())",
                new Func<MethodCallExpression>(() => {
                    var x = new MethodCallExpression { MethodName = "TestMethod" };
                    x.Arguments.Add(new Argument { Expression = new ConstantExpression() { Value = 123 } });
                    x.Arguments.Add(new Argument { Expression = new NewObjectExpression() { Type = typeof(System.IO.MemoryStream) } });
                    return x;
                })
            },
            new object[] {
                "TestMethod(ref num, out str)",
                new Func<MethodCallExpression>(() => {
                    var x = new MethodCallExpression { MethodName = "TestMethod" };
                    x.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "num"} },
                        Modifier = MethodParameterModifier.Ref
                    });
                    x.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "str"} },
                        Modifier = MethodParameterModifier.Out
                    });
                    return x;
                })
            },
            new object[] {
                "obj.TestMethod(num, str)",
                new Func<MethodCallExpression>(() => {
                    var x = new MethodCallExpression {
                        MethodName = "TestMethod",
                        Target = new LocalVariableExpression() {
                           Variable = new LocalVariable { Name = "obj" }
                        }
                    };
                    x.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "num"} },
                    });
                    x.Arguments.Add(new Argument {
                        Expression = new LocalVariableExpression() { Variable = new LocalVariable { Name = "str"} },
                    });
                    return x;
                })
            },
            new object[] {
                "this.TestMethod()",
                new Func<MethodCallExpression>(() => {
                    var x = new MethodCallExpression {
                        Method = new MethodMember(MemberVisibility.Public, "TestMethod", new MethodSignature()),
                        Target = new ThisExpression()
                    };
                    return x;
                })
            },
            new object[] {
                "base.TestMethod()",
                new Func<MethodCallExpression>(() => {
                    var x = new MethodCallExpression {
                        Method = new MethodMember(MemberVisibility.Public, "TestMethod", new MethodSignature()),
                        Target = new BaseExpression()
                    };
                    return x;
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitMethodCallExpression))]
        public void CanEmitMethodCallExpression(string expectedCode, Func<MethodCallExpression> expressionFactory)
        {
            //-- arrange

            var expression = expressionFactory();

            //-- act

            var actualSyntax = ExpressionSyntaxEmitter.EmitSyntax(expression);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanEmitAssignmentExpression = new object[][] {
            #region Test cases
            new object[] {
                "x = 123",
                new AssignmentExpression {
                    Left = new LocalVariableExpression { Variable = new LocalVariable { Name = "x" } },
                    Right = new ConstantExpression { Value = 123 }
                }
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitAssignmentExpression))]
        public void CanEmitAssignmentExpression(string expectedCode, AssignmentExpression expression)
        {
            //-- arrange

            var enclosingMethod = new MethodMember(MemberVisibility.Public, "Method1", new MethodSignature());
            enclosingMethod.Body = new BlockStatement(
                new ExpressionStatement { Expression = expression }
            );

            //-- act

            var actualSyntax = new MethodSyntaxEmitter(enclosingMethod).EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode("public void Method1() { " + expectedCode + "; }");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanEmitNewArrayExpression = new object[][] {
            #region Test cases
            new object[] {
                "new int[123]",
                new Func<AbstractExpression>(() => new NewArrayExpression {
                    ElementType = typeof(int),
                    Length = new ConstantExpression { Value = 123 }
                })
            },
            new object[] {
                "new int[0]",
                new Func<AbstractExpression>(() => new NewArrayExpression {
                    ElementType = typeof(int),
                    Length = new ConstantExpression { Value = 0 }
                })
            },
            new object[] {
                "new int[] { 1, 2, 3 }",
                new Func<AbstractExpression>(() => {
                    var expression = new NewArrayExpression {
                         ElementType = typeof(int),
                    };
                    expression.DimensionInitializerValues.Add(new List<AbstractExpression> {
                        new ConstantExpression { Value = 1 },
                        new ConstantExpression { Value = 2 },
                        new ConstantExpression { Value = 3 }
                    });
                    return expression;
                })
            },
            new object[] {
                "new int[] { }",
                new Func<AbstractExpression>(() => {
                    var expression = new NewArrayExpression {
                         ElementType = typeof(int),
                    };
                    expression.DimensionInitializerValues.Add(new List<AbstractExpression>());
                    return expression;
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitNewArrayExpression))]
        public void CanEmitNewArrayExpression(string expectedCode, Func<AbstractExpression> expressionFactory)
        {
            //-- arrange

            var expression = expressionFactory();

            var enclosingMethod = new MethodMember(MemberVisibility.Public, "Method1", new MethodSignature());
            enclosingMethod.Body = new BlockStatement(
                new ExpressionStatement { Expression = expression }
            );

            //-- act

            var actualSyntax = new MethodSyntaxEmitter(enclosingMethod).EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode("public void Method1() { " + expectedCode + "; }");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanEmitIndexerExpression = new object[][] {
            #region Test cases
            new object[] {
                "a[123]",
                new Func<IndexerExpression>(() => {
                    var varA = new LocalVariable { Name = "a"};
                    var indexer = new IndexerExpression { Target = new LocalVariableExpression { Variable = varA } };
                    indexer.IndexArguments.Add(new ConstantExpression { Value = 123 });
                    return indexer;
                })
            },
            new object[] {
                "a[123]",
                new Func<IndexerExpression>(() => {
                    var varA = new LocalVariable { Name = "a"};
                    var indexer = new IndexerExpression {
                        Target = new LocalVariableExpression { Variable = varA },
                        Index = new ConstantExpression { Value = 123 }
                    };
                    return indexer;
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitIndexerExpression))]
        public void CanEmitIndexerExpression(string expectedCode, Func<IndexerExpression> expressionFactory)
        {
            //-- arrange

            var expression = expressionFactory();

            var enclosingMethod = new MethodMember(MemberVisibility.Public, "Method1", new MethodSignature());
            enclosingMethod.Body = new BlockStatement(
                new ExpressionStatement { Expression = expression }
            );

            //-- act

            var actualSyntax = new MethodSyntaxEmitter(enclosingMethod).EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode("public void Method1() { " + expectedCode + "; }");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanEmitMemberExpression = new object[][] {
            #region Test cases
            new object[] {
                "this.IntValue",
                new Func<MemberExpression>(() => new MemberExpression {
                    Target = new ThisExpression(),
                    Member = new FieldMember { Name = "IntValue" }
                })
            },
            new object[] {
                "obj.IntValue",
                new Func<MemberExpression>(() => new MemberExpression {
                    Target = new LocalVariableExpression { Variable = new LocalVariable { Name = "obj" } },
                    Member = new FieldMember { Name = "IntValue" }
                })
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitMemberExpression))]
        public void CanEmitMemberExpression(string expectedCode, Func<MemberExpression> expressionFactory)
        {
            //-- arrange

            var expression = expressionFactory();

            var enclosingMethod = new MethodMember(MemberVisibility.Public, "Method1", new MethodSignature());
            enclosingMethod.Body = new BlockStatement(
                new ExpressionStatement { Expression = expression }
            );

            //-- act

            var actualSyntax = new MethodSyntaxEmitter(enclosingMethod).EmitSyntax();

            //-- assert

            actualSyntax.Should().BeEquivalentToCode("public void Method1() { " + expectedCode + "; }");
        }
    }
}
