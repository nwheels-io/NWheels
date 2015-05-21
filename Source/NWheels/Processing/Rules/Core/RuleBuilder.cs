using System;
using System.Linq.Expressions;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Core
{
    public class RuleBuilder<TDataContext>
    {
        public RuleBuilder<TDataContext> Action<TAction>(params Func<OperandBuilder, RuleSystemDescription.Operand>[] values) where TAction : IRuleAction
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static RuleBuilder<TDataContext> DefineRule(string idName = null, string description = null)
        {
            return new RuleBuilder<TDataContext>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleBuilder<TDataContext> If(System.Action<ConditionBuilder> condition)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleBuilder<TDataContext> Always()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator RuleSystemDescription.Rule(RuleBuilder<TDataContext> builder)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConditionBuilder
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OperandBuilder
        {
            public OperandBuilder Variable(Expression<Func<TDataContext, object>> contextProperty)
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperandBuilder Constant(object value)
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperandBuilder DivideBy()
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OperandBuilder PercentOf()
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static implicit operator RuleSystemDescription.Operand(OperandBuilder builder)
            {
                throw new NotImplementedException();
            }
        }
    }
}
