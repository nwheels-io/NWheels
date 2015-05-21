using System;
using System.Linq.Expressions;
using NWheels.Processing.Jobs;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Core
{
    public abstract class RuleSystemBuilder
    {
        public abstract RuleSystemDescription GetDescription(bool includeDomain = true, bool includeRules = true);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleSystemBuilder<TContext> : RuleSystemBuilder
    {
        public void AddAllContextPropertiesAsVariables()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddContextVariables(params Expression<Func<TContext, object>>[] contextProperties)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(Func<TContext, TValue> variable, string idName, string description)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(IRuleVariable<TContext, TValue> variable)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction(IRuleFunction function)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<TReturn>(Func<TReturn> function, string idName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, TReturn>(Func<T1, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, TReturn>(
            Func<T1, T2, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, T3, TReturn>(
            Func<T1, T2, T3, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, T3, T4, TReturn>(
            Func<T1, T2, T3, T4, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction(IRuleAction action)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction(Action<TContext> action, string idName, string description)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1>(
            Action<TContext, T1> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2>(
            Action<TContext, T1, T2> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2, T3>(
            Action<TContext, T1, T2, T3> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2, T3, T4>(
            Action<TContext, T1, T2, T3, T4> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddRuleSet(
            string idName,
            string description,
            RuleSystemDescription.RuleSetMode mode = RuleSystemDescription.RuleSetMode.ApplyFirstMatch,
            bool failIfNotMatched = false,
            RuleSystemDescription.Operand precondition = null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddMetaRule(IMetaRule rule)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddMetaRule(
            string idName,
            string description,
            string[] parameterNames,
            Type[] parameterTypes,
            string[] parameterDescriptions)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImportStandardMathFunctions()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImportStandardStatisticFunctions()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImportStandardDateTimeFunctions()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImportStandardStringFunctions()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override RuleSystemDescription GetDescription(bool includeDomain = true, bool includeRules = true)
        {
            throw new NotImplementedException();
        }
    }
}
