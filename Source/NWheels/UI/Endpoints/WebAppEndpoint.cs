using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Endpoints
{
    public interface IWebAppEndpoint
    {
        IUiApplication App { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WebAppEndpoint<TApp> : IWebAppEndpoint 
        where TApp : IUiApplication
    {
        private readonly TApp _app;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebAppEndpoint(TApp app)
        {
            _app = app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IUiApplication App
        {
            get { return _app; }
        }
    }
}
