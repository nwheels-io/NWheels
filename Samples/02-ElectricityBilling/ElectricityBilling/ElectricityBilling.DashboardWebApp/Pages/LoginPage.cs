using System;
using System.Collections.Generic;
using System.Text;
using NWheels.UI;
using NWheels.UI.Components;
using NWheels.UI.Web;

namespace ElectricityBilling.DashboardWebApp.Pages
{
    public class LoginPage : WebPage<Empty.ViewModel>
    {
        public override void Controller(ClientSideScript script, Empty.ViewModel model)
        {
            //TBD...
            //script.OnLoad(() => script.Navigate)
        }

        public ViewComponent<Empty.ViewModel> VisibleComponent { get; }



        public class LoginComponent
        {
        }

        public class ForgotPasswordComponent
        {
        }

        public class SignUpComponent
        {
        }

    }
}
