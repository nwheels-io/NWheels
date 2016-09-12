using System;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Processing;
using NWheels.UI;

namespace NWheels.Samples.MyMusicDB.Domain
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class InteractiveLoginTx : ITransactionScript<Empty.Context, InteractiveLoginTx.IInput, UserLoginTransactionScript.Result>
    {
        private readonly UserLoginTransactionScript _loginTx;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InteractiveLoginTx(UserLoginTransactionScript loginTx)
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