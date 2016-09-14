using System;
using NWheels.DataObjects;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Domains.Security.UI
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class InteractiveForgotPasswordTx : ITransactionScript<Empty.Context, InteractiveForgotPasswordTx.IInput, InteractiveForgotPasswordTx.IOutput>
    {
        private readonly ResetPasswordTransactionScript _resetPasswordTx;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InteractiveForgotPasswordTx(ResetPasswordTransactionScript resetPasswordTx, IViewModelObjectFactory viewModelFactory)
        {
            _resetPasswordTx = resetPasswordTx;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,Input,Result>

        public IInput InitializeInput(Empty.Context context)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Preview(IInput input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Execute(IInput input)
        {
            _resetPasswordTx.Execute(input.LoginName);

            var output = _viewModelFactory.NewEntity<IOutput>();
            output.Message = "If we find a matching account, an email will be sent to email address on your record, in order to allow you reset your password.";
            return output;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required, PropertyContract.Semantic.LoginName]
            string LoginName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            string Message { get; set; }
        }
    }
}
