using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class UserLoginScreen : ScreenBase<UserLoginScreen, Empty.Input, Empty.Data, Empty.State>
    {
        private UidlScreen _passwordExpiredScreen;
        private UidlScreen _forgotPasswordScreen;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetPasswordExpiredScreen(UidlScreen passwordExpiredScreen)
        {
            _passwordExpiredScreen = passwordExpiredScreen;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetForgotPasswordScreen(UidlScreen forgotPasswordScreen)
        {
            _forgotPasswordScreen = forgotPasswordScreen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserLoginScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.SignInSignUp;

            ContentRoot = Splash;
            Splash.InsideContent = LoginForm;

            LoginForm.Text = "SignIn";
            LoginForm.HelpText = "LogInToYourAccount";
            LoginForm.TemplateName = "TransactionFormLoginStyle";
            LoginForm.InputForm.TemplateName = "FormLoginStyle";
            LoginForm.UserAlertDisplayMode = UserAlertDisplayMode.Inline;
            LoginForm.Execute.Text = "Login";
            LoginForm.Commands.Remove(LoginForm.Reset);

            presenter.On(LoginForm.OutputReady).ActivateSessionTimeout(vm => vm.Input.IdleSessionExpiryMinutes);

            if (_passwordExpiredScreen != null)
            {
                presenter.On(LoginForm.OperationFailed)
                    .Switch(vm => vm.Input.FaultCode)
                        .When(LoginFault.PasswordExpired.ToString(), b => b.Navigate().ToScreenNonTyped(_passwordExpiredScreen))
                    .EndSwitch();
            }

            if (_forgotPasswordScreen != null)
            {
                LoginForm.Commands.Add(ForgotPassword);
                ForgotPassword.Kind = CommandKind.Navigate;
                ForgotPassword.Severity = CommandSeverity.None;
                ForgotPassword.UIStyle = CommandUIStyle.Link;
                presenter.On(ForgotPassword).Navigate().ToScreenNonTyped(_forgotPasswordScreen);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Splash Splash { get; set; }
        public TransactionForm<InteractiveLoginTx.IInput, InteractiveLoginTx, UserLoginTransactionScript.Result> LoginForm { get; set; }
        public UidlCommand ForgotPassword { get; set; }
    }
}
