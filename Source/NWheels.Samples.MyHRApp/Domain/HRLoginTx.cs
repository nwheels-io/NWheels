using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Processing;
using NWheels.UI;

namespace NWheels.Samples.MyHRApp.Domain
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class HRLoginTx : ITransactionScript<Empty.Context, HRLoginTx.IInput, UserLoginTransactionScript.Result>
    {
        private readonly UserLoginTransactionScript _loginTx;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HRLoginTx(UserLoginTransactionScript loginTx)
        {
            _loginTx = loginTx;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,Input,Result>

        public IInput InitializeInput(Empty.Context context)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript.Result Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript.Result Execute(IInput input)
        {
            var output = _loginTx.Execute(input.LoginName, input.Password);
            return output;
        }

        public void SaveInputDraft(IInput input)
        {
            throw new NotImplementedException();
        }

        public void DiscardInputDraft()
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required, PropertyContract.Semantic.LoginName]
            string LoginName { get; set; }

            [PropertyContract.Required, PropertyContract.Semantic.Password]
            string Password { get; set; }
        }
    }
}