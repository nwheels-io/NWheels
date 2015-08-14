using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NWheels.Processing.Rules.Core
{
    public abstract class RuleSystemBuilder
    {
        public abstract RuleSystemDescription GetDescription(bool includeDomain = true, bool includeRules = true);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleSystemBuilder<TContext> : RuleSystemBuilder
    {
        private readonly RuleSystemDescription _desciption = new RuleSystemDescription();
		private readonly CompiledRuleSystem<TContext> _compiledRuleSystem =new CompiledRuleSystem<TContext>();

        private readonly Dictionary<string, IRuleDomainObject> _domainObjectsByIdName = new Dictionary<string, IRuleDomainObject>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------



        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAllContextPropertiesAsVariables()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddScalarPropertiesAsVariables<TObject>(Expression<Func<TContext, TObject>> objectInContext)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddContextVariables(params Expression<Func<TContext, object>>[] contextProperties)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(Func<TContext, TValue> variable, string idName, string description)
        {
            var runtimeVariable = new RuleVariable<TContext, TValue>(onGetValue: variable, onGetInventoryValues: null, idName: idName, description: description);
            AddVariable<TValue>(runtimeVariable);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(Func<TContext, TValue> variable, Func<IList<object>> inventoryVariables, string idName, string description)
        {
            var runtimeVariable = new RuleVariable<TContext, TValue>(
                onGetValue: variable, 
                onGetInventoryValues: inventoryVariables, 
                idName: idName, 
                description: description);
	        
            AddVariable<TValue>(runtimeVariable);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(IRuleVariable<TContext, TValue> variable)
        {
			var metaVariable = new RuleSystemDescription.DomainVariable {
				IdName = variable.IdName ,
				Description = variable.Description,
				ValueType = RuleSystemDescription.TypeDescription.Of<TValue>()
			};
			_desciption.Domain.Variables.Add(metaVariable);
			_compiledRuleSystem.Variables.Add(variable);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction(IRuleFunction function)
        {
	        var metaFunction = new RuleSystemDescription.DomainFunction {
		        Description = function.Description,
		        IdName = function.Description,
		        ValueType = RuleSystemDescription.TypeDescription.Of(function.GetType()),
				Parameters = null
	        };
			
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
            return _desciption;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompiledRuleSystem<TContext> CompileRuleSystem(RuleSystemData rules)
        {
            return _compiledRuleSystem;
        }
    }
}
