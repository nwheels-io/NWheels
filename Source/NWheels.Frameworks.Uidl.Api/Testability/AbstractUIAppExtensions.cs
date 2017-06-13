using NWheels.Frameworks.Uidl.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Testability
{
    public static class AbstractUIAppExtensions
    {
        public static WebUIAppDriver<TApp> Driver<TApp>(this TApp app)
            where TApp : class, IWebApp
        {
            return new WebUIAppDriver<TApp>();
        }
    }
}
