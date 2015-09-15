using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Processing.Rules.Core;
using NWheels.Processing.Rules.Impl;

namespace NWheels.Processing.Rules
{
    public class CompiledRuleSystem<TDataContext>
    {
        private readonly RuleSystemDescription _description;
        private readonly RuleSystemData _ruleData;
        private readonly Dictionary<string, IRuleDomainObject> _runtimeObjectByIdName;
        private readonly Dictionary<string, RuleSystemDescription.DomainObject> _objectDescriptionByIdName;
        private readonly Dictionary<string, RuleSystemDescription.RuleSetDescription> _ruleSetDescriptionByIdName;
        private readonly IRuleEngineLogger _logger;

        //----------------------------------------------------------------------------------------------------------------------
	
		public CompiledRuleSystem(
            IRuleEngineLogger logger, 
            RuleSystemDescription description, 
            RuleSystemData ruleData,
            IDictionary<string, IRuleDomainObject> runtimeObjects)
		{
            _logger = logger;
            _description = description;
		    _ruleData = ruleData;
            _runtimeObjectByIdName = new Dictionary<string, IRuleDomainObject>(runtimeObjects);
		    _objectDescriptionByIdName = description.BuildObjectByIdNameLookup(logger);
		    _ruleSetDescriptionByIdName = description.RuleSets.ToDictionary(rs => rs.IdName);
		}

        //----------------------------------------------------------------------------------------------------------------------
	    
        public void Run(TDataContext dataContext)
        {
            var runner = new SlowLateBoundInterpretingRunner(this);
            runner.RunRuleSystem(dataContext);
        }

        //----------------------------------------------------------------------------------------------------------------------

        internal void Add(IRuleDomainObject runtimeObject)
        {
            if ( _runtimeObjectByIdName.ContainsKey(runtimeObject.IdName) )
            {
                throw _logger.DuplicateDomainObjectName(
                    runtimeObject.IdName, 
                    duplicate: runtimeObject.GetType(), 
                    existing: _runtimeObjectByIdName[runtimeObject.IdName].GetType());
            }

            _runtimeObjectByIdName.Add(runtimeObject.IdName, runtimeObject);
        }

        //----------------------------------------------------------------------------------------------------------------------
        
        private RuleSystemDescription Description
        {
            get { return _description; }
        }

        //----------------------------------------------------------------------------------------------------------------------
        
        private IReadOnlyDictionary<string, IRuleDomainObject> RuntimeObjectByIdName
        {
            get { return _runtimeObjectByIdName; }
        }

        //----------------------------------------------------------------------------------------------------------------------

        private IReadOnlyDictionary<string, RuleSystemDescription.DomainObject> ObjectDescriptionByIdName
        {
            get { return _objectDescriptionByIdName; }
        }

        //----------------------------------------------------------------------------------------------------------------------

        private IReadOnlyDictionary<string, RuleSystemDescription.RuleSetDescription> RuleSetDescriptionByIdName
        {
            get { return _ruleSetDescriptionByIdName; }
        }

        //----------------------------------------------------------------------------------------------------------------------

        private IRuleEngineLogger Logger
        {
            get { return _logger; }
        }

        //----------------------------------------------------------------------------------------------------------------------

        private RuleSystemData RuleData
        {
            get { return _ruleData; }
        }

        //----------------------------------------------------------------------------------------------------------------------

        public class ActionContext : IRuleActionContext<TDataContext>
        {
            public ActionContext(
                TDataContext data, 
                RuleSystemDescription.Rule rule, 
                RuleSystemDescription.RuleSet ruleSet,
                RuleSystemDescription.DomainAction action,
                RuleSystemDescription.ActionInvocation invocation)
            {
                Data = data;
                RuleSetIdName = ruleSet.IdName;
                RuleSetDescription = ruleSet.IdName;
                RuleIdName = rule.IdName;
                RuleDescription = rule.IdName;
                ArgumentDescriptions = action.Parameters.Select(p => p.Description ?? p.Name).ToArray();
            }

             //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IRuleActionContext<TDataContext>

            public TDataContext Data { get; private set; }
            public string RuleSetIdName { get; private set; }
            public string RuleSetDescription { get; private set; }
            public string RuleIdName { get; private set; }
            public string RuleDescription { get; private set; }
            public IReadOnlyList<string> ArgumentDescriptions { get; private set; }

            #endregion
        }

        //----------------------------------------------------------------------------------------------------------------------

        private class SlowLateBoundInterpretingRunner
        {
            private readonly CompiledRuleSystem<TDataContext> _ruleSystem;
            private EvaluationContext<TDataContext> _evaluationContext;
            private TDataContext _dataContext;
            private RuleSystemDescription.RuleSet _currentRuleSet;
            private RuleSystemDescription.Rule _currentRule;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SlowLateBoundInterpretingRunner(CompiledRuleSystem<TDataContext> ruleSystem)
            {
                _ruleSystem = ruleSystem;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RunRuleSystem(TDataContext dataContext)
            {
                _dataContext = dataContext;
                _evaluationContext = new EvaluationContext<TDataContext>(
                    _ruleSystem.ObjectDescriptionByIdName, 
                    _ruleSystem.RuntimeObjectByIdName, 
                    _dataContext);

                using ( var activity = _ruleSystem.Logger.RunRuleSystem(name: _ruleSystem.Description.IdName, context: dataContext.ToString()) )
                {
                    try
                    {
                        for ( int index = 0 ; index < _ruleSystem.RuleData.RuleSets.Count ; index++ )
                        {
                            RunRuleSet(index, _ruleSystem.RuleData.RuleSets[index]);
                        }
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunRuleSet(int index, RuleSystemDescription.RuleSet ruleSet)
            {
                using ( var activity = _ruleSystem.Logger.RunRuleSet(index, ruleSet.IdName) )
                {
                    _currentRuleSet = ruleSet;

                    try
                    {
                        var ruleSetDescription = _ruleSystem.RuleSetDescriptionByIdName[ruleSet.IdName];

                        if ( ruleSetDescription.Precondition != null && !EvaluateRuleSetPrecondition(ruleSetDescription.Precondition) )
                        {
                            return;
                        }

                        bool anyRuleMatched;
                        RunRulesInRuleSet(ruleSet, ruleSetDescription, out anyRuleMatched);

                        if ( ruleSetDescription.FailIfNotMatched && !anyRuleMatched )
                        {
                            throw _ruleSystem.Logger.NoRuleMatchedInRuleSet(name: ruleSet.IdName);
                        }
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool EvaluateRuleSetPrecondition(RuleSystemDescription.Operand operand)
            {
                var result = (bool)operand.Evaluate(_evaluationContext);
                
                _ruleSystem.Logger.RuleSetPreconditionEvaluated(result);
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunRulesInRuleSet(
                RuleSystemDescription.RuleSet ruleSet, 
                RuleSystemDescription.RuleSetDescription ruleSetDescription, 
                out bool anyRuleMatched)
            {
                anyRuleMatched = false;

                for ( int ruleIndex = 0 ; ruleIndex < ruleSet.Rules.Count ; ruleIndex++ )
                {
                    bool ruleMatched;
                    RunRule(ruleIndex, ruleSet.Rules[ruleIndex], out ruleMatched);

                    anyRuleMatched |= ruleMatched;

                    if ( ruleMatched && ruleSetDescription.Mode == RuleSystemDescription.RuleSetMode.ApplyFirstMatch )
                    {
                        _ruleSystem.Logger.ExitRuleSetOnFirstRuleMatch();
                        break;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunRule(int index, RuleSystemDescription.Rule rule, out bool matched)
            {
                using ( var activity = _ruleSystem.Logger.RunRule(index, rule.IdName) )
                {
                    _currentRule = rule;

                    try
                    {
                        matched = EvaluateRuleCondition(rule);
                        
                        if ( matched )
                        {
                            ApplyRuleActions(rule);
                        }
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool EvaluateRuleCondition(RuleSystemDescription.Rule rule)
            {
                var result = (bool)rule.Condition.Evaluate(_evaluationContext);
                _ruleSystem.Logger.RuleConditionEvaluated(result);
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ApplyRuleActions(RuleSystemDescription.Rule rule)
            {
                using ( var activity = _ruleSystem.Logger.ApplyRuleActions() )
                {
                    try
                    {
                        for ( int index = 0 ; index < rule.Actions.Count ; index++ )
                        {
                            InvokeRuleAction(index, rule.Actions[index]);
                        }
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void InvokeRuleAction(int index, RuleSystemDescription.ActionInvocation invocation)
            {
                using ( var activity = _ruleSystem.Logger.ApplyRuleAction(index, invocation.IdName) )
                {
                    try
                    {
                        var runtimeAction = (IRuleAction)_ruleSystem.RuntimeObjectByIdName[invocation.IdName];
                        var actionDescription = (RuleSystemDescription.DomainAction)_ruleSystem.ObjectDescriptionByIdName[invocation.IdName];
                        var actionContext = new ActionContext(_dataContext, _currentRule, _currentRuleSet, actionDescription, invocation);

                        invocation.InvokeLateBound(actionDescription, runtimeAction, _evaluationContext, actionContext);
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }
        }
    }
}
