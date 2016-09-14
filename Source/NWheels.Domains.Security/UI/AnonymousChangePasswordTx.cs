using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Processing;
using NWheels.UI;
using System.Security;
using NWheels.UI.Factories;

namespace NWheels.Domains.Security.UI
{
    [SecurityCheck.AllowAnonymous]
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class AnonymousChangePasswordTx : TransactionScript<Empty.Context, AnonymousChangePasswordTx.IInput, AnonymousChangePasswordTx.IOutput>
    {
        private readonly ChangePasswordTransactionScript _innerTx;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AnonymousChangePasswordTx(ChangePasswordTransactionScript innerTx, IViewModelObjectFactory viewModelFactory)
        {
            _innerTx = innerTx;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,Output>

        public override IOutput Execute(IInput input)
        {
            _innerTx.Execute(input.LoginName, input.OldPassword, input.NewPassword, passwordExpired: true);

            var output = _viewModelFactory.NewEntity<IOutput>();
            output.Message = "Password successfully changed.";
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
            string OldPassword { get; set; }

            [PropertyContract.Required, PropertyContract.Semantic.Password]
            string NewPassword { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            string Message { get; set; }
        }
    }
}
