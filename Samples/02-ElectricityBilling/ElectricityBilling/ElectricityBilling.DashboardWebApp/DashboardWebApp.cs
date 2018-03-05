using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.DashboardWebApp.Pages;
using NWheels.UI;
using NWheels.UI.Web;

namespace ElectricityBilling.DashboardWebApp
{
    public class DashboardWebApp : WebApp<Empty.Session, Empty.Args> 
    {
        private LoginPage _login { get; }

        public override void Controller()
        {
            //OnInit 

            base.Controller();
        }
    }
}
