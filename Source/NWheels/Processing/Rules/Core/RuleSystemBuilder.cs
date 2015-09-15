using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hapil;

namespace NWheels.Processing.Rules.Core
{
    public abstract class RuleSystemBuilder
    {
        public abstract RuleSystemDescription GetDescription();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleSystemBuilder<TContext> : RuleSystemBuilder
    {
        private readonly RuleSystemDescription _description;
        private readonly IRuleEngineLogger _logger;
        private readonly Dictionary<string, IRuleDomainObject> _runtimeObjectByIdName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleSystemBuilder(Type codeBehindType, IRuleEngineLogger logger)
        {
            _logger = logger;
            _description = new RuleSystemDescription();
            _runtimeObjectByIdName = new Dictionary<string, IRuleDomainObject>();

            if ( codeBehindType != null )
            {
                _description.IdName = codeBehindType.FriendlyName();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetIdName(string idName)
        {
            _description.IdName = idName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAllContextPropertiesAsVariables()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddScalarPropertiesAsVariables<TObject>(Expression<Func<TContext, TObject>> objectInContext)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddContextVariables(params Expression<Func<TContext, object>>[] contextProperties)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(Func<TContext, TValue> variable, string idName, string description = null)
        {
            var runtimeVariable = new RuleVariable<TContext, TValue>(onGetValue: variable, onGetInventoryValues: null, idName: idName, description: description);
            AddVariable<TValue>(runtimeVariable);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddVariable<TValue>(Func<TContext, TValue> variable, Func<IList<object>> inventoryValues, string idName, string description = null)
        {
            var runtimeVariable = new RuleVariable<TContext, TValue>(
                onGetValue: variable, 
                onGetInventoryValues: inventoryValues, 
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

			_description.Domain.Variables.Add(metaVariable);
            _runtimeObjectByIdName[variable.IdName] = variable;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction(IRuleFunction function)
        {
	        var metaFunction = new RuleSystemDescription.DomainFunction {
		        Description = function.Description,
		        IdName = function.Description,
		        ValueType = RuleSystemDescription.TypeDescription.Of(function.GetType()),
				Parameters = new List<RuleSystemDescription.ParameterDescription>()
	        };
			
            _description.Domain.Functions.Add(metaFunction);
            _runtimeObjectByIdName[function.IdName] = function;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<TReturn>(Func<TReturn> function, string idName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, TReturn>(Func<T1, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, TReturn>(
            Func<T1, T2, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, T3, TReturn>(
            Func<T1, T2, T3, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFunction<T1, T2, T3, T4, TReturn>(
            Func<T1, T2, T3, T4, TReturn> function, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction(IRuleAction action)
        {
            var metaAction = new RuleSystemDescription.DomainAction() {
                IdName = action.IdName,
                Description = action.Description,
                Parameters = new List<RuleSystemDescription.ParameterDescription>() //TODO: support actions with parameters
            };

            _description.Domain.Actions.Add(metaAction);
            _runtimeObjectByIdName[action.IdName] = action;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction(Action<TContext> action, string idName, string description = null)
        {
            AddAction(new RuleAction<TContext>(idName, description, ctx => action(ctx.Data)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1>(
            Action<TContext, T1> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
            AddAction(new RuleAction<TContext, T1>(idName, description, (ctx, p1) => action(ctx.Data, p1)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2>(
            Action<TContext, T1, T2> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
            AddAction(new RuleAction<TContext, T1, T2>(idName, description, (ctx, p1, p2) => action(ctx.Data, p1, p2)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2, T3>(
            Action<TContext, T1, T2, T3> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
            AddAction(new RuleAction<TContext, T1, T2, T3>(idName, description, (ctx, p1, p2, p3) => action(ctx.Data, p1, p2, p3)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddAction<T1, T2, T3, T4>(
            Action<TContext, T1, T2, T3, T4> action, string idName, string description, string[] parameterNames, string[] parameterDescriptions = null)
        {
            AddAction(new RuleAction<TContext, T1, T2, T3, T4>(idName, description, (ctx, p1, p2, p3, p4) => action(ctx.Data, p1, p2, p3, p4)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddRuleSet(
            string idName,
            string description = null,
            RuleSystemDescription.RuleSetMode mode = RuleSystemDescription.RuleSetMode.ApplyFirstMatch,
            bool failIfNotMatched = false,
            RuleSystemDescription.Operand precondition = null)
        {
            _description.RuleSets.Add(new RuleSystemDescription.RuleSetDescription {
                IdName = idName,
                Description = description,
                Mode = mode,
                FailIfNotMatched = failIfNotMatched,
                Precondition = precondition
            });
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

        public override RuleSystemDescription GetDescription()
        {
            return _description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompiledRuleSystem<TContext> CompileRuleSystem(RuleSystemData rules)
        {
            return new CompiledRuleSystem<TContext>(_logger, _description, rules, _runtimeObjectByIdName);
        }
    }
}
