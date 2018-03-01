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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Controller()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoginComponent : BaseComponent<LoginComponent.LoginModel>
        {
            [TransactionComponent.Configure(BindToParentModel = true)]
            private readonly TransactionComponent<LoginModel> _loginTx;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Controller()
            {
                _loginTx.OnSubmit.Subscribe(this, () => Script.BLOCK(() => {

                    Script.IF(() => Model.RememberMe,
                        () => Script.MutateModel(() => new LoginModel {
                            LoginCookie = null
                        })
                    );

                    Script.GetServerComponent<IElectricityBillingContext>()
                        .Invoke(x => x
                            .CustomerLoginTx(_loginTx.Model.Email, _loginTx.Model.Password));

                    Script.RaiseEvent(OnLoggedIn);

                }));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClientEvent OnLoggedIn { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class LoginModel
            {
                [NWheels.MemberContract.Semantics.EmailAddress]
                public string Email { get; set; }

                [NWheels.MemberContract.Semantics.PasswordClear]
                public string Password { get; set; }

                [NWheels.UI.MemberContract.Storage.ClientMachine]
                public bool RememberMe { get; set; }

                [NWheels.UI.MemberContract.Storage.ClientMachine]
                public string LoginCookie { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForgotPasswordComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SignUpComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ChangePasswordComponent
        {
        }
    }
}
