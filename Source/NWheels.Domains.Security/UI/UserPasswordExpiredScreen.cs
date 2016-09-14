using NWheels.Extensions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class UserPasswordExpiredScreen : ScreenBase<UserPasswordExpiredScreen, Empty.Input, Empty.Data, Empty.State>
    {
        private UidlScreen _loginScreen;
        private UidlScreen _forgotPasswordScreen;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UserPasswordExpiredScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetLoginScreen(UidlScreen loginScreen)
        {
            _loginScreen = loginScreen;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetForgotPasswordScreen(UidlScreen forgotPasswordScreen)
        {
            _forgotPasswordScreen = forgotPasswordScreen;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserPasswordExpiredScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.SignInSignUp;

            ContentRoot = Splash;
            Splash.InsideContent = ChangePasswordForm;

            ChangePasswordForm.Text = "PasswordExpired";
            ChangePasswordForm.HelpText = "YourPasswordHasExpiredAndMustBeChanged";
            ChangePasswordForm.TemplateName = "TransactionFormLoginStyle";
            ChangePasswordForm.InputForm.TemplateName = "FormLoginStyle";
            ChangePasswordForm.UserAlertDisplayMode = UserAlertDisplayMode.Popup;
            ChangePasswordForm.Execute.Text = "ChangePassword";
            ChangePasswordForm.Commands.Remove(ChangePasswordForm.Reset);
            ChangePasswordForm.InputForm.Field(x => x.NewPassword, modifiers: FormFieldModifiers.Password | FormFieldModifiers.Confirm);

            ChangePasswordForm.UseOutputForm();
            ChangePasswordForm.OutputForm.TemplateName = "FormAlertBig";
            ChangePasswordForm.OutputForm.Field(x => x.Message, type: FormFieldType.Alert, setup: f => {
                f.Label = null; // use field value for alert contents
                f.AlertType = UserAlertType.Success;
            });

            if (_loginScreen != null)
            {
                ChangePasswordForm.OutputForm.Commands.Add(LogInToMyAccount);
                LogInToMyAccount.Kind = CommandKind.Navigate;
                LogInToMyAccount.Severity = CommandSeverity.None;
                LogInToMyAccount.UIStyle = CommandUIStyle.Link;
                presenter.On(LogInToMyAccount).Navigate().ToScreenNonTyped(_loginScreen);
            }

            if (_forgotPasswordScreen != null)
            {
                ChangePasswordForm.Commands.Add(ForgotPassword);
                ForgotPassword.Kind = CommandKind.Navigate;
                ForgotPassword.Severity = CommandSeverity.None;
                ForgotPassword.UIStyle = CommandUIStyle.Link;
                presenter.On(ForgotPassword).Navigate().ToScreenNonTyped(_forgotPasswordScreen);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Splash Splash { get; set; }
        public TransactionForm<AnonymousChangePasswordTx.IInput, AnonymousChangePasswordTx, AnonymousChangePasswordTx.IOutput> ChangePasswordForm { get; set; }
        public UidlCommand ForgotPassword { get; set; }
        public UidlCommand LogInToMyAccount { get; set; }
    }
}
