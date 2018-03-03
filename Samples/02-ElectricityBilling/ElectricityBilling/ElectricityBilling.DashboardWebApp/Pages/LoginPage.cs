using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain;
using NWheels.UI;
using NWheels.UI.Components;
using NWheels.UI.Web;

namespace ElectricityBilling.DashboardWebApp.Pages
{
    [NWheels.UI.TypeContract.TemplateUrl("theme://page?type=login")]
    public class LoginPage : WebPage<Empty.Model>
    {
        [FrameComponent.Configure(
            TemplatePlaceholder = "Form", 
            InitialView = typeof(LoginComponent))]
        private readonly FrameComponent<Empty.Model> _formFrame;

        private readonly LoginComponent _login;
        private readonly ForgotPasswordComponent _forgotPassword;
        private readonly SignUpComponent _signUp;
        private readonly ChangePasswordComponent _changePassword;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Controller()
        {
            var g = this.CodeWriter;

            _login.OnForgotPassword.Subscribe(_formFrame.NavigateTo(_forgotPassword));
            _login.OnSignUp.Subscribe(_formFrame.NavigateTo(_signUp));
            _login.OnPasswordExpired.Subscribe(_formFrame.NavigateTo(_changePassword));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoginComponent : BaseComponent<LoginComponent.LoginModel>
        {
            [TransactionComponent.Configure(BindToParentModel = true)]
            private readonly TransactionComponent<LoginModel> _loginTx;

            private readonly Event<CustomerLoginResult> _onLoggedIn;
            private readonly Event<CustomerLoginResult> _onPasswordExpired;

            [Command.Configure(Importance = CommandImportance.Utility)]
            private readonly Command _forgotPassword;

            [Command.Configure(Importance = CommandImportance.Utility)]
            private readonly Command _signUp;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Controller()
            {
                var g = this.CodeWriter;

                this.OnInit.Subscribe(
                    g.IF(() => Model.RememberMe && Model.LoginCookie != null).THEN(
                        g.RETURN(
                            g.CALL<IElectricityBillingContext>(
                                x => x.CustomerLoginByCookieTx(Model.LoginCookie)
                            )
                            .THEN(loginResult => g.RAISE_EVENT(_onLoggedIn, loginResult))
                        )
                    )
                );

                _loginTx.OnSubmit.Subscribe(
                    g.MUTATE_MODEL(() => new LoginModel { LoginCookie = null }),
                    g.RETURN(
                        g.CALL<IElectricityBillingContext>(
                            x => x.CustomerLoginTx(Model.Email, Model.Password, Model.RememberMe)
                        ).THEN<CustomerLoginResult>(loginResult => 
                            g.MUTATE_MODEL(() => new LoginModel {
                                LoginCookie = loginResult.Value.LoginCookie,
                                LoginResult = loginResult.Value
                            }
                        ))
                    )
                );

                _loginTx.OnCompleted.Subscribe(
                    g.RAISE_EVENT(_onLoggedIn, () => Model.LoginResult)
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Event<CustomerLoginResult> OnLoggedIn => _onLoggedIn;
            public Event OnPasswordExpired => _onPasswordExpired;
            public Event OnForgotPassword => _forgotPassword.OnExecute;
            public Event OnSignUp => _signUp.OnExecute;

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

        public class ForgotPasswordComponent : BaseComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SignUpComponent : BaseComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChangePasswordComponent : BaseComponent
        {
        }
    }
}
