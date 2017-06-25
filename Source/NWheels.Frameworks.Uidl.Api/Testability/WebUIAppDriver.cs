using NWheels.Frameworks.Uidl.Abstractions;
using NWheels.Frameworks.Uidl.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Uidl.Testability
{
    public class WebUIAppDriver<TApp> : AbstractUIAppDriver<TApp>, IWebUIAppDriver
        where TApp : class, IAbstractUIApp
    {
        public Task NavigateToStartPage()
        {
            return Task.CompletedTask;
        }

        public IWebPage CurrentPage()
        {
            return null;
        }

        public TPage CurrentPage<TPage>() where TPage : class, IWebPage
        {
            return null;
        }
    }

    public interface IWebUIAppDriver
    {
    }
}
