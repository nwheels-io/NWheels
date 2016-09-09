using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Domains.Security;
using NWheels.Samples.MyHRApp.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyHRApp.UI
{
    public class HRLoginScreen : ScreenBase<HRLoginScreen, Empty.Input, Empty.Data, Empty.State>
    {
        public HRLoginScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<HRLoginScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.SignInSignUp;

            ContentRoot = Splash;
            Splash.Text = "HR";
            Splash.HelpText = "ExampleApplication";
            Splash.InsideContent = LoginForm;


            LoginForm.Text = "SignIn";
            LoginForm.HelpText = "LogInToYourAccount";
            LoginForm.TemplateName = "TransactionFormLoginStyle";
            LoginForm.InputForm.TemplateName = "FormLoginStyle";
            LoginForm.UserAlertDisplayMode = UserAlertDisplayMode.Inline;
            LoginForm.Execute.Text = "Login";
            LoginForm.Commands.Remove(LoginForm.Reset);

            presenter.On(LoginForm.OutputReady).ActivateSessionTimeout(vm => vm.Input.IdleSessionExpiryMinutes);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Splash Splash { get; set; }
        public TransactionForm<HRLoginTx.IInput, HRLoginTx, UserLoginTransactionScript.Result> LoginForm { get; set; }
    }

}