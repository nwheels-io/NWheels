using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.DashboardWebApp.Pages;
using NWheels.UI;
using NWheels.UI.Web;

namespace ElectricityBilling.DashboardWebApp
{
    public class DashboardWebApp : WebApp<Empty.Session, LoginPage.SignUpArgs> 
    {
        private LoginPage _login { get; }

        protected override void Controller()
        {
            OnNavigatedHere += (args) => NavigateTo(_login, args);

        }
    }
}
