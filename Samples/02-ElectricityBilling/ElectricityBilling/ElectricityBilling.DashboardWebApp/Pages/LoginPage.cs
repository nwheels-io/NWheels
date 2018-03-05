using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Accounts;
using NWheels.UI;
using NWheels.UI.Components;
using NWheels.UI.Web;

namespace ElectricityBilling.DashboardWebApp.Pages
{
    [NWheels.UI.TypeContract.TemplateUrl("theme://page?type=login")]
    public class LoginPage : WebPage<Empty.Model, Empty.Args>
    {
        [FrameComponent.Configure(TemplatePlaceholder = "Form")]
        private readonly FrameComponent<Empty.Model> _formFrame;

        private readonly LoginComponent _loginComponent;
        private readonly ForgotPasswordComponent _forgotPasswordComponent;
        private readonly SignUpComponent _signUpComponent;
        private readonly ChangePasswordComponent _changePasswordComponent;

        [Command.Configure(Importance = CommandImportance.Utility)]
        private readonly Command _forgotPassword;

        [Command.Configure(Importance = CommandImportance.Utility)]
        private readonly Command _signUp;

        [Command.Configure(Importance = CommandImportance.Utility)]
        private readonly Command _logIn;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Controller()
        {
            OnInit += () => _formFrame.NavigateTo(_loginComponent);

            _loginComponent.OnPasswordExpired += (args) => _formFrame.NavigateTo(_changePasswordComponent, args);
            _forgotPassword.OnExecute += () => _formFrame.NavigateTo(_forgotPasswordComponent);
            _signUp.OnExecute += () => _formFrame.NavigateTo(_signUpComponent);
            _logIn.OnExecute += () => _formFrame.NavigateTo(_loginComponent);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<CustomerLoginResult> OnLoggedIn;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoginComponent : BaseComponent<LoginComponent.LoginModel>
        {
            [NWheels.UI.MemberContract.BindToParentModel]
            private readonly TransactionComponent<LoginModel> _loginTx;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoginComponent()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Controller()
            {
                OnInit += async () => {
                    if (Model.RememberMe && Model.LoginCookie != null)
                    {
                        Model.LoginResult = await ServerComponent<IElectricityBillingContext>().CustomerLoginByCookieTx(Model.LoginCookie);
                        OnLoggedIn(Model.LoginResult);
                    }
                };

                _loginTx.OnSubmit += async () => {
                    Model.LoginCookie = null;

                    try
                    {
                        Model.LoginResult = await ServerComponent<IElectricityBillingContext>().CustomerLoginByCookieTx(Model.LoginCookie);
                        Model.LoginCookie = Model.LoginResult.LoginCookie;
                    }
                    catch (ServerFaultException<LoginFault> error) when (error.Code == LoginFault.PasswordExpired)
                    {
                        OnPasswordExpired(new EmailArgs {
                            Email = Model.Email
                        });
                    }
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<CustomerLoginResult> OnLoggedIn;
            public event Action<EmailArgs> OnPasswordExpired;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class LoginModel
            {
                [NWheels.UI.MemberContract.Storage.ClientMachine]
                [NWheels.MemberContract.Presentation.Hidden]
                public string LoginCookie { get; set; }

                [NWheels.MemberContract.Semantics.EmailAddress]
                public string Email { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear]
                public string Password { get; set; }

                [NWheels.UI.MemberContract.Storage.ClientMachine]
                public bool RememberMe { get; set; }

                [NWheels.UI.MemberContract.Output]
                public CustomerLoginResult LoginResult { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmailArgs
        {
            public string Email { get; set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForgotPasswordComponent : BaseComponent<ForgotPasswordComponent.ForgotPasswordModel>
        {
            public class ForgotPasswordModel
            {
                [NWheels.MemberContract.Semantics.EmailAddress]
                public string Email { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SignUpComponent : BaseComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChangePasswordComponent : NavigationTargetComponent<ChangePasswordComponent.ChangePasswordModel, EmailArgs>
        {
            [TransactionComponent.Configure(BindToParentModel = true)]
            private readonly TransactionComponent<ChangePasswordModel> _changePasswordTx;

            private readonly ILocalizables _localizables;

            public ChangePasswordComponent(ILocalizables localizables)
            {
                _localizables = localizables;
            }

            public override void Controller()
            {
                OnNavigatedHere += (args) => {
                    Model.Email = args.Email;
                };

                _changePasswordTx.OnSubmit += async () => {
                    await ServerComponent<IElectricityBillingContext>().CustomerChangePasswordTx(Model.Email, Model.OldPassword, Model.NewPassword);
                    await Alert(AlertSeverity.Success, AlertBehavior.Toast, _localizables.PasswordSuccessfullyChanged);
                    Done();
                };
            }

            public event Action Done;

            public class ChangePasswordModel
            {
                [NWheels.MemberContract.Semantics.EmailAddress]
                public string Email { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear]
                public string OldPassword { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear(ShouldConfirm = true)]
                public string NewPassword { get; set; }
            }

            public interface ILocalizables
            {
                string PasswordSuccessfullyChanged { get; }
            }
        }
    }
}
