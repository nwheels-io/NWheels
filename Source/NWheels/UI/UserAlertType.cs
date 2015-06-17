using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public enum UserAlertType
    {
        Info,
        Success,
        Warning,
        Error
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class UserAlertTypeAttribute : Attribute
    {
        protected UserAlertTypeAttribute(UserAlertType alertType, UserAlertResult[] alertResults)
        {
            this.AlertType = alertType;
            this.AlertResults = alertResults;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAlertType AlertType { get; private set; }
        public UserAlertResult[] AlertResults { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InfoAlertAttribute : UserAlertTypeAttribute
    {
        public InfoAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Info, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SuccessAlertAttribute : UserAlertTypeAttribute
    {
        public SuccessAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Success, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WarningAlertAttribute : UserAlertTypeAttribute
    {
        public WarningAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Warning, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ErrorAlertAttribute : UserAlertTypeAttribute
    {
        public ErrorAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Error, alertResults)
        {
        }
    }
}
