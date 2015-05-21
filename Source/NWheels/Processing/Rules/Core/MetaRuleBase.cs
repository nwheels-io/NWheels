using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Processing.Jobs;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;
using NWheels.Utilities;

namespace NWheels.Processing.Core
{
    public abstract class MetaRuleBase : IMetaRule
    {
        private readonly Delegate _ruleFactory;
        private readonly Type[] _parameterTypes;
        private readonly List<RuleSystemDescription.ParameterDescription> _parameters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MetaRuleBase(string idName, string description, Delegate ruleFactory, params Type[] parameterTypes)
        {
            this.IdName = idName;
            this.Description = description;

            _ruleFactory = ruleFactory;
            _parameterTypes = parameterTypes;
            _parameters = new List<RuleSystemDescription.ParameterDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MetaRuleBase(string idName, string description, Func<IEnumerable<RuleSystemDescription.Rule>> ruleFactory)
            : this(idName, description, ruleFactory, new Type[0])
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        RuleSystemDescription.Rule[] IMetaRule.CreateRules(string[] parameterValues)
        {
            return InvokeRuleFactory(parameterValues);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IParameterizedRuleDomainObject.DescribeParameters(out RuleSystemDescription.ParameterDescription[] parameters)
        {
            parameters = _parameters.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string IdName { get; private set; }
        public string Description { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void DescribeParameter(string name, string description)
        {
            var nextParameterType = _parameterTypes[_parameters.Count];
            _parameters.Add(new RuleSystemDescription.ParameterDescription() {
                Type = RuleSystemDescription.TypeDescription.Of(nextParameterType),
                Name = name,
                Description = description
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal virtual RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<IEnumerable<RuleSystemDescription.Rule>>)_ruleFactory;
            return concreteFactory().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        internal Delegate RuleFactory
        {
            get { return _ruleFactory; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(ParseUtility.Parse<T1>(parameterValues[0])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3, T4> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, T4, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3), typeof(T4))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, T4, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2]),
                ParseUtility.Parse<T4>(parameterValues[3])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3, T4, T5> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, T4, T5, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, T4, T5, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2]),
                ParseUtility.Parse<T4>(parameterValues[3]),
                ParseUtility.Parse<T5>(parameterValues[4])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3, T4, T5, T6> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, T4, T5, T6, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, T4, T5, T6, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2]),
                ParseUtility.Parse<T4>(parameterValues[3]),
                ParseUtility.Parse<T5>(parameterValues[4]),
                ParseUtility.Parse<T6>(parameterValues[5])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3, T4, T5, T6, T7> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, T4, T5, T6, T7, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2]),
                ParseUtility.Parse<T4>(parameterValues[3]),
                ParseUtility.Parse<T5>(parameterValues[4]),
                ParseUtility.Parse<T6>(parameterValues[5]),
                ParseUtility.Parse<T7>(parameterValues[6])).ToArray();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MetaRuleBase<T1, T2, T3, T4, T5, T6, T7, T8> : MetaRuleBase
    {
        protected MetaRuleBase(string idName, string description, Func<T1, T2, T3, T4, T5, T6, T7, T8, RuleSystemDescription.Rule> ruleFactory)
            : base(idName, description, ruleFactory, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override RuleSystemDescription.Rule[] InvokeRuleFactory(string[] parameterValues)
        {
            var concreteFactory = (Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<RuleSystemDescription.Rule>>)base.RuleFactory;
            return concreteFactory(
                ParseUtility.Parse<T1>(parameterValues[0]),
                ParseUtility.Parse<T2>(parameterValues[1]),
                ParseUtility.Parse<T3>(parameterValues[2]),
                ParseUtility.Parse<T4>(parameterValues[3]),
                ParseUtility.Parse<T5>(parameterValues[4]),
                ParseUtility.Parse<T6>(parameterValues[5]),
                ParseUtility.Parse<T7>(parameterValues[6]),
                ParseUtility.Parse<T8>(parameterValues[7])).ToArray();
        }
    }
}
