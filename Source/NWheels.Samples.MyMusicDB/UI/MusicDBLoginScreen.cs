using NWheels.Domains.Security;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyMusicDB.UI
{
    public class MusicDBLoginScreen : ScreenBase<MusicDBLoginScreen, Empty.Input, Empty.Data, Empty.State>
    {
        public MusicDBLoginScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<MusicDBLoginScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.SignInSignUp;

            ContentRoot = Splash;
            Splash.Text = "MusicDB";
            Splash.HelpText = "ExampleApplication";
            Splash.InsideContent = LoginForm;
            Splash.PoweredBy = "PoweredByNWHEELS";

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
        public TransactionForm<InteractiveLoginTx.IInput, InteractiveLoginTx, UserLoginTransactionScript.Result> LoginForm { get; set; }
    }

}