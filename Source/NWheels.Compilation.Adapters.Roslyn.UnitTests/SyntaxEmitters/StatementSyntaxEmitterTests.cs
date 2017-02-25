using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class StatementSyntaxEmitterTests : SyntaxEmittingTestBase
    {
        public static IEnumerable<object[]> TestCases_CanEmitStatementSyntax = new object[][] {
            #region Test cases
            new object[] {
                "return 123;",
                new Func<AbstractStatement>(() => new ReturnStatement { Expression = new ConstantExpression { Value = 123 } })
            },
            new object[] {
                "throw new System.NotImplementedException();",
                new Func<AbstractStatement>(() => new ThrowStatement { Exception = new NewObjectExpression { Type = typeof(NotImplementedException) } })
            },
            new object[] {
                "a = 123;",
                new Func<AbstractStatement>(() => {
                    var varA = new LocalVariable { Name = "a"};
                    return new ExpressionStatement { Expression = new AssignmentExpression {
                        Left = new LocalVariableExpression { Variable = varA },
                        Right = new ConstantExpression { Value = 123 }
                    } };
                })
            },
            new object[] {
                "int a;",
                new Func<AbstractStatement>(() => new VariableDeclarationStatement {
                    Variable = new LocalVariable { Type = typeof(int), Name = "a" },
                })
            },
            new object[] {
                "var a = 123;",
                new Func<AbstractStatement>(() => new VariableDeclarationStatement {
                    Variable = new LocalVariable { Name = "a" },
                    InitialValue = new ConstantExpression { Value = 123 }
                })
            },
            new object[] {
                "int a = 123;",
                new Func<AbstractStatement>(() => new VariableDeclarationStatement {
                    Variable = new LocalVariable { Type = typeof(int), Name = "a" },
                    InitialValue = new ConstantExpression { Value = 123 }
                })
            },
            new object[] {
                "{ a = 123; b = 456; return a; }",
                new Func<AbstractStatement>(() => {
                    var varA = new LocalVariable { Name = "a"};
                    var varB = new LocalVariable { Name = "b" };
                    return new BlockStatement(new AbstractStatement[] {
                        new ExpressionStatement { Expression = new AssignmentExpression {
                            Left = new LocalVariableExpression { Variable = varA },
                            Right = new ConstantExpression { Value = 123 }
                        } },
                        new ExpressionStatement { Expression = new AssignmentExpression {
                            Left = new LocalVariableExpression { Variable = varB },
                            Right = new ConstantExpression { Value = 456 }
                        } },
                        new ReturnStatement { Expression = new LocalVariableExpression { Variable = varA } }
                    });
                })
            }
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanEmitStatementSyntax))]
        public void CanEmitStatementSyntax(string expectedCode, Func<AbstractStatement> statementFactory)
        {
            //-- arrange

            var statement = statementFactory();

            //-- act

            var actualSyntax = StatementSyntaxEmitter.EmitSyntax(statement);

            //-- assert

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }
    }
}
