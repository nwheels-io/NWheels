using System;
using System.Collections.Concurrent;
using NWheels.Processing;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public class CqrsAppControllerBase<TAppModel>
        where TAppModel : CqrsAppModelBase
    {
        private readonly object _sessionSyncRoot = new object();
        private readonly ICqrsClient _clientApi;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CqrsAppControllerBase(ICqrsClient clientApi)
        {
            _clientApi = clientApi;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BeginConnect(string userName, string password)
        {
            lock (_sessionSyncRoot)
            {
                if (Session != null)
                {
                    throw new InvalidOperationException("Already connected.");
                }

                _clientApi.Connect(userName, password);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BeginDisconnect()
        {
            lock (_sessionSyncRoot)
            {
                if (Session == null)
                {
                    throw new InvalidOperationException("Not connected.");
                }

                _clientApi.Disconnect();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CqrsAppSession<TAppModel> Session { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        //public event EventHandler<AppSessionEventArgs<TAppModel>> SessionChanged;
        //public event EventHandler<AppModelEventArgs<TAppModel>> UpdatingViews;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BeginCommand(IServerCommand command, Action<ICommandProcessedEvent> onCompleted = null, TimeSpan? timeout = null)
        {
            lock (_sessionSyncRoot)
            {
                if (Session == null)
                {
                    throw new InvalidOperationException("Not connected.");
                }

                _clientApi.SendCommands(new[] { command });
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class AppSessionEventArgs<TAppModel> : EventArgs
        where TAppModel : CqrsAppModelBase
    {
        public AppSessionEventArgs(CqrsAppSession<TAppModel> session)
        {
            Session = session;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CqrsAppSession<TAppModel> Session { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class AppModelEventArgs<TAppModel> : EventArgs
        where TAppModel : CqrsAppModelBase
    {
        public AppModelEventArgs(TAppModel model)
        {
            Model = model;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAppModel Model { get; private set; }
    }
}
