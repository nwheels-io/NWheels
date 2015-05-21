using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Core
{
    public abstract class RuleActionBase : IRuleAction, IParameterizedRuleDomainObject
    {
        private readonly Type[] _parameterTypes;
        private readonly List<RuleSystemDescription.ParameterDescription> _parameters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RuleActionBase(string idName, string description, params Type[] parameterTypes)
        {
            this.IdName = idName;
            this.Description = description;

            _parameterTypes = parameterTypes;
            _parameters = new List<RuleSystemDescription.ParameterDescription>();
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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext> : RuleActionBase, IRuleAction<TDataContext>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1> : RuleActionBase, IRuleAction<TDataContext, T1>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2> : RuleActionBase, IRuleAction<TDataContext, T1, T2>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3, T4> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3, T4, T5> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3, T4, T5, T6> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3, T4, T5, T6, T7> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(
            IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class RuleActionBase<TDataContext, T1, T2, T3, T4, T5, T6, T7, T8> : 
        RuleActionBase, 
        IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        protected RuleActionBase(string idName, string description)
            : base(idName, description)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Apply(
            IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8);
    }
}
