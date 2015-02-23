using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.UI.Endpoints
{
    public interface IWebAppEndpoint
    {
        IUiApplication Contract { get; }
        string Address {  get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class WebAppEndpoint
    {
        protected string GetConfiguredAddress(IUiApplication contract)
        {
            return "http://localhost:" + Interlocked.Increment(ref _s_lastUsedPortNumber) + "/"; 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int _s_lastUsedPortNumber = 8900;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WebAppEndpoint<TApp> : WebAppEndpoint, IWebAppEndpoint 
        where TApp : IUiApplication
    {
        private readonly TApp _app;
        private readonly string _address;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebAppEndpoint(TApp app)
        {
            _app = app;
            _address = GetConfiguredAddress(app);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IUiApplication Contract
        {
            get { return _app; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Address
        {
            get
            {
                return _address;
            }
        }
    }
}
