using System;
using System.Collections.Generic;

namespace NWheels.Processing.Rules.Core
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

    public class RuleAction<TDataContext> : RuleActionBase, IRuleAction<TDataContext>
    {
        private readonly Action<IRuleActionContext<TDataContext>> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context)
        {
            _onApply(context);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1> : RuleActionBase, IRuleAction<TDataContext, T1>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1)
        {
            _onApply(context, param1);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2> : RuleActionBase, IRuleAction<TDataContext, T1, T2>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2)
        {
            _onApply(context, param1, param2);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3)
        {
            _onApply(context, param1, param2, param3);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3, T4> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            _onApply(context, param1, param2, param3, param4);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3, T4, T5> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            _onApply(context, param1, param2, param3, param4, param5);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3, T4, T5, T6> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            _onApply(context, param1, param2, param3, param4, param5, param6);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7> : RuleActionBase, IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6, T7> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6, T7> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(
            IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            _onApply(context, param1, param2, param3, param4, param5, param6, param7);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7, T8> : 
        RuleActionBase, 
        IRuleAction<TDataContext, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        private readonly Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6, T7, T8> _onApply;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleAction(string idName, string description, Action<IRuleActionContext<TDataContext>, T1, T2, T3, T4, T5, T6, T7, T8> onApply)
            : base(idName, description)
        {
            _onApply = onApply;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(
            IRuleActionContext<TDataContext> context, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            _onApply(context, param1, param2, param3, param4, param5, param6, param7, param8);
        }
    }
}
