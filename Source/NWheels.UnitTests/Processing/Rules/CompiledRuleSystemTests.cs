using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Processing.Rules
{
    [TestFixture]
    public class CompiledRuleSystemTests : UnitTestBase
    {
        [Test]
        public void CanCompileAndRunRuleSystem()
        {
            //-- arrange

            var logger = Framework.Logger<IRuleEngineLogger>();
            var builder = new RuleSystemBuilder<TestDataContext>(typeof(TestCodeBehind), logger);
            
            var codeBehind = new TestCodeBehind();
            codeBehind.BuildRuleSystem(builder);

            var compiledRuleSystem = builder.CompileRuleSystem(CreateTestRules());

            //-- act

            var dataContext = new TestDataContext(input: 123);
            compiledRuleSystem.Run(dataContext);

            //-- assert

            dataContext.ActionLog.ShouldBe(new[] { "ActionPositive", "ActionOdd", "ActionSecondHundred" });

            foreach ( var logMessage in Framework.TakeLog() )
            {
                Console.WriteLine(logMessage.SingleLineText);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private RuleSystemData CreateTestRules()
        {
            return new RuleSystemData() {
                RuleSets = new List<RuleSystemDescription.RuleSet> {
                    new RuleSystemDescription.RuleSet() {
                        IdName = "RuleSetSign",
                        Rules = new List<RuleSystemDescription.Rule>() {
                            #region R1
                            new RuleSystemDescription.Rule() {
                                IdName = "R1",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "Input" },
                                    Operator = RuleSystemDescription.Operator.GreaterThan,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(0)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionPositive",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                            #region R2
                            new RuleSystemDescription.Rule() {
                                IdName = "R2",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "Input" },
                                    Operator = RuleSystemDescription.Operator.LessThan,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(0)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionNegative",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                            #region R3
                            new RuleSystemDescription.Rule() {
                                IdName = "R3",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "Input" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(0)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionZero",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                        }
                    },
                    new RuleSystemDescription.RuleSet() {
                        IdName = "RuleSetEvenOdd",
                        Rules = new List<RuleSystemDescription.Rule>() {
                            #region R4
                            new RuleSystemDescription.Rule() {
                                IdName = "R4",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "Even" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(true)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionEven",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                            #region R5
                            new RuleSystemDescription.Rule() {
                                IdName = "R5",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "Even" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(false)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionOdd",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                        }
                    },
                    new RuleSystemDescription.RuleSet() {
                        IdName = "RuleSetHundred",
                        Rules = new List<RuleSystemDescription.Rule>() {
                            #region R6
                            new RuleSystemDescription.Rule() {
                                IdName = "R6",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "AbsoluteHundred" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(1)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionFirstHundred",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                            #region R7
                            new RuleSystemDescription.Rule() {
                                IdName = "R7",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "AbsoluteHundred" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(2)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionSecondHundred",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                            #region R8
                            new RuleSystemDescription.Rule() {
                                IdName = "R8",
                                Condition = new RuleSystemDescription.BinaryExpressionOperand() {
                                    Left = new RuleSystemDescription.VariableOperand() { IdName = "AbsoluteHundred" },
                                    Operator = RuleSystemDescription.Operator.Equal,
                                    Right = RuleSystemDescription.ConstantOperand.FromValue(3)
                                },
                                Actions = new List<RuleSystemDescription.ActionInvocation>() {
                                    new RuleSystemDescription.ActionInvocation() {
                                        IdName = "ActionThirdHundred",
                                        Parameters = new List<RuleSystemDescription.Operand>()
                                    }
                                }
                            },
                            #endregion
                        }
                    },
                }
            };
        }
    }
}
