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
    public class LoginPage : WebPage<Empty.Model>
    {
        [FrameComponent.Configure(
            TemplatePlaceholder = "Form", 
            InitialView = typeof(LoginComponent))]
        private readonly FrameComponent<Empty.Model> _formFrame;

        private readonly LoginComponent _loginComponent;
        private readonly ForgotPasswordComponent _forgotPasswordComponent;
        private readonly SignUpComponent _signUpComponent;
        private readonly ChangePasswordComponent _changePasswordComponent;

        [Command.Configure(Importance = CommandImportance.Utility)]
        private Command _forgotPassword { get; }

        [Command.Configure(Importance = CommandImportance.Utility)]
        private Command _signUp { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Controller()
        {
            var g = this.CodeWriter;

            _loginComponent.OnPasswordExpired.Subscribe(args => 
                _formFrame.NavigateTo(_changePasswordComponent, () => args.Value));

            _signUp.OnExecute.Subscribe(_formFrame.NavigateTo(_signUpComponent));
            _forgotPassword.OnExecute.Subscribe(_formFrame.NavigateTo(_forgotPasswordComponent));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Event<CustomerLoginResult> OnLoggedIn => _loginComponent.OnLoggedIn;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoginComponent : BaseComponent<LoginComponent.LoginModel>
        {
            [TransactionComponent.Configure(BindToParentModel = true)]
            private readonly TransactionComponent<LoginModel> _loginTx;

            [Command.Configure(Importance = CommandImportance.Utility)]
            private readonly Command _forgotPassword;

            [Command.Configure(Importance = CommandImportance.Utility)]
            private readonly Command _signUp;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoginComponent()
            {
                
            }

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
                            .THEN(loginResult => g.FIRE(OnLoggedIn, loginResult))
                        )
                    )
                );

                _loginTx.OnSubmit.Subscribe(
                    g.MUTATE(Model, () => new LoginModel { LoginCookie = null }),
                    g.RETURN(
                        g.CALL<IElectricityBillingContext>(
                            x => x.CustomerLoginTx(Model.Email, Model.Password, Model.RememberMe)
                        ).THEN<CustomerLoginResult>(loginResult => 
                            g.MUTATE(Model, () => new LoginModel {
                                LoginCookie = loginResult.Value.LoginCookie,
                                LoginResult = loginResult.Value
                            }
                        ))
                        .CATCH<LoginFault>(fault => g.BEGIN(
                            g.IF(() => fault.Value == LoginFault.PasswordExpired).THEN(
                                g.FIRE(OnPasswordExpired, () => new PasswordExpiredEventArgs {
                                    Email = Model.Email
                                })
                            ).ELSE(
                                g.THROW()
                            )        
                        ))
                    )
                );

                _loginTx.OnCompleted.Subscribe(
                    g.FIRE(OnLoggedIn, () => Model.LoginResult)
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Event<CustomerLoginResult> OnLoggedIn { get; }
            public Event<PasswordExpiredEventArgs> OnPasswordExpired { get; }

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

        public class PasswordExpiredEventArgs
        {
            public string Email { get; set; }
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

        public class ChangePasswordComponent : NavigationTargetComponent<ChangePasswordComponent.ChangePasswordModel, PasswordExpiredEventArgs>
        {
            [TransactionComponent.Configure(BindToParentModel = true)]
            private readonly TransactionComponent<ChangePasswordModel> _changePasswordTx;

            private readonly AlertComponent _passwordExpiredAlert;

            public override void Controller()
            {
                var g = CodeWriter;

                OnNavigatedHere.Subscribe(args => g.BEGIN(
                    g.MUTATE(Model, () => new ChangePasswordModel {
                        Email = args.Value.Email
                    })
                ));
            }

            public class ChangePasswordModel
            {
                [NWheels.MemberContract.Semantics.EmailAddress]
                public string Email { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear]
                public string OldPassword { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear(ShouldConfirm = true)]
                public string NewPassword { get; set; }
            }
        }
    }
}
