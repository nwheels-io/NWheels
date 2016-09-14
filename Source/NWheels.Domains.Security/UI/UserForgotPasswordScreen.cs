using NWheels.Globalization;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class UserForgotPasswordScreen : ScreenBase<UserForgotPasswordScreen, Empty.Input, Empty.Data, Empty.State>
    {
        private UidlScreen _loginScreen;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UserForgotPasswordScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetLoginScreen(UidlScreen loginScreen)
        {
            _loginScreen = loginScreen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserForgotPasswordScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.SignInSignUp;

            ContentRoot = Splash;
            Splash.InsideContent = ForgotPasswordForm;

            ForgotPasswordForm.Text = "ForgotPassword";
            ForgotPasswordForm.HelpText = "WeNeedYourLoginName";
            ForgotPasswordForm.TemplateName = "TransactionFormLoginStyle";
            ForgotPasswordForm.InputForm.TemplateName = "FormLoginStyle";
            ForgotPasswordForm.UserAlertDisplayMode = UserAlertDisplayMode.Popup;
            ForgotPasswordForm.Execute.Text = "Continue";
            ForgotPasswordForm.Commands.Remove(ForgotPasswordForm.Reset);

            ForgotPasswordForm.UseOutputForm();
            ForgotPasswordForm.OutputForm.TemplateName = "FormAlertBig";
            ForgotPasswordForm.OutputForm.Field(x => x.Message, type: FormFieldType.Alert, setup: f => {
                f.Label = null; // use field value for alert contents
                f.AlertType = UserAlertType.Success;
            });

            if (_loginScreen != null)
            {
                ForgotPasswordForm.Commands.Add(LoginToMyAccount);
                ForgotPasswordForm.OutputForm.Commands.Add(LoginToMyAccount);
                LoginToMyAccount.Kind = CommandKind.Navigate;
                LoginToMyAccount.Severity = CommandSeverity.None;
                LoginToMyAccount.UIStyle = CommandUIStyle.Link;
                presenter.On(LoginToMyAccount).Navigate().ToScreenNonTyped(_loginScreen);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Splash Splash { get; set; }
        public TransactionForm<InteractiveForgotPasswordTx.IInput, InteractiveForgotPasswordTx, InteractiveForgotPasswordTx.IOutput> ForgotPasswordForm { get; set; }
        public UidlCommand LoginToMyAccount { get; set; }
    }
}
