using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Rules.Core
{
	public abstract class RuleFunctionBase : IRuleFunction
	{
	    protected RuleFunctionBase(string idName, string description)
	    {
	        IdName = idName;
	        Description = description;
	    }

	    //-----------------------------------------------------------------------------------------------------------------------------------------------------

		#region IRuleDomainObject Members

		public string IdName { get; private set; }
        public string Description { get; private set; }

	    #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

	    object IRuleFunction.GetValue(object[] arguments)
	    {
	        return GetValueNonTyped(arguments);
	    }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

	    protected abstract object GetValueNonTyped(object[] arguments);
	}

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleFunction<TReturn> : RuleFunctionBase, IRuleFunction<TReturn>
    {
        private readonly Func<TReturn> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleFunction(string idName, string description, Func<TReturn> onGetValue)
            : base(idName, description)
        {
            _onGetValue = onGetValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleFunction<TReturn>

        public TReturn GetValue()
        {
            return _onGetValue();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Overrides of RuleFunctionBase

        protected override object GetValueNonTyped(object[] arguments)
        {
            return GetValue();
        }

        #endregion  
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleFunction<TArg1, TReturn> : RuleFunctionBase, IRuleFunction<TArg1, TReturn>
    {
        private readonly Func<TArg1, TReturn> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleFunction(string idName, string description, Func<TArg1, TReturn> onGetValue)
            : base(idName, description)
        {
            _onGetValue = onGetValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleFunction<TReturn>

        public TReturn GetValue(TArg1 arg1)
        {
            return _onGetValue(arg1);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RuleFunctionBase

        protected override object GetValueNonTyped(object[] arguments)
        {
            return GetValue((TArg1)arguments[0]);
        }

        #endregion
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleFunction<TArg1, TArg2, TReturn> : RuleFunctionBase, IRuleFunction<TArg1, TArg2, TReturn>
    {
        private readonly Func<TArg1, TArg2, TReturn> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleFunction(string idName, string description, Func<TArg1, TArg2, TReturn> onGetValue)
            : base(idName, description)
        {
            _onGetValue = onGetValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleFunction<TReturn>

        public TReturn GetValue(TArg1 arg1, TArg2 arg2)
        {
            return _onGetValue(arg1, arg2);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RuleFunctionBase

        protected override object GetValueNonTyped(object[] arguments)
        {
            return GetValue(
                (TArg1)arguments[0],
                (TArg2)arguments[1]);
        }

        #endregion
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleFunction<TArg1, TArg2, TArg3, TReturn> : RuleFunctionBase, IRuleFunction<TArg1, TArg2, TArg3, TReturn>
    {
        private readonly Func<TArg1, TArg2, TArg3, TReturn> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleFunction(string idName, string description, Func<TArg1, TArg2, TArg3, TReturn> onGetValue)
            : base(idName, description)
        {
            _onGetValue = onGetValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleFunction<TReturn>

        public TReturn GetValue(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return _onGetValue(arg1, arg2, arg3);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RuleFunctionBase

        protected override object GetValueNonTyped(object[] arguments)
        {
            return GetValue(
                (TArg1)arguments[0],
                (TArg2)arguments[1],
                (TArg3)arguments[2]);
        }

        #endregion
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RuleFunction<TArg1, TArg2, TArg3, TArg4, TReturn> : RuleFunctionBase, IRuleFunction<TArg1, TArg2, TArg3, TArg4, TReturn>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, TReturn> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleFunction(string idName, string description, Func<TArg1, TArg2, TArg3, TArg4, TReturn> onGetValue)
            : base(idName, description)
        {
            _onGetValue = onGetValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleFunction<TReturn>

        public TReturn GetValue(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return _onGetValue(arg1, arg2, arg3, arg4);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RuleFunctionBase

        protected override object GetValueNonTyped(object[] arguments)
        {
            return GetValue(
                (TArg1)arguments[0],
                (TArg2)arguments[1],
                (TArg3)arguments[2],
                (TArg4)arguments[3]);
        }

        #endregion
    }
}
