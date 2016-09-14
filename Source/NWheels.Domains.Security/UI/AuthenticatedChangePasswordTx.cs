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
using NWheels.Globalization;
using NWheels.Globalization.Core;
using NWheels.UI.Factories;

namespace NWheels.Domains.Security.UI
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class AuthenticatedChangePasswordTx : TransactionScript<Empty.Context, AuthenticatedChangePasswordTx.IInput, AuthenticatedChangePasswordTx.IOutput>
    {
        private readonly ChangePasswordTransactionScript _innerTx;
        private readonly ISecurityDomainLogger _logger;
        private readonly IViewModelObjectFactory _viewModelFactory;
        //private readonly ILocalizables _localizables;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AuthenticatedChangePasswordTx(
            ChangePasswordTransactionScript innerTx, 
            ISecurityDomainLogger logger, 
            IViewModelObjectFactory viewModelFactory) 
            //ILocalizables localizables)
        {
            _innerTx = innerTx;
            _logger = logger;
            _viewModelFactory = viewModelFactory;
            //_localizables = localizables;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,Output>

        public override IOutput Execute(IInput input)
        {
            IUserAccountEntity loggedInUser;
            
            try
            {
                loggedInUser = Session.Current.GetUserAccountAs<IUserAccountEntity>();
            }
            catch (Exception e)
            {
                _logger.UserNotLoggedIn(Session.Current.Id, e);
                throw new SecurityException("Not logged in.");
            }

            _innerTx.Execute(loggedInUser.LoginName, input.OldPassword, input.NewPassword, passwordExpired: false);

            var output = _viewModelFactory.NewEntity<IOutput>();
            output.Message = "Password successfully changed";
            return output;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILocalizables : ILocalizableApplicationResources
        {
            string PasswordSuccessfullyChanged { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
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
