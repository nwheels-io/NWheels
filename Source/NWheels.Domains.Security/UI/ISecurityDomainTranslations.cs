using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Globalization;

namespace NWheels.Domains.Security.UI
{
    public interface ISecurityDomainTranslations : ILocalizableApplicationResources
    {
        string LoginName { get; }
        string Password { get; }
        string SignUp { get; }
        string ForgotPassword { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class HardCoded_ISecurityDomainTranslations : ISecurityDomainTranslations
    {
        public HardCoded_ISecurityDomainTranslations()
        {
            this.LoginName = "LoginName ";
            this.Password = "Password";
            this.SignUp = "Sign Up";
            this.ForgotPassword = "Forgot Password";
        }

        #region Implementation of ISecurityDomainTranslations

        public string LoginName { get; private set; }
        public string Password { get; private set; }
        public string SignUp { get; private set; }
        public string ForgotPassword { get; private set; }

        #endregion
    }
}
