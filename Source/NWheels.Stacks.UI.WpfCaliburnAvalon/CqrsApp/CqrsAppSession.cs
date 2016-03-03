using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Caliburn.Micro;
using NWheels.Authorization.Core;
using NWheels.Processing;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public class CqrsAppSession<TAppModel>
        where TAppModel : CqrsAppModelBase
    {
        private readonly ConcurrentDictionary<int, CommandResult> _resultByCommandIndex;
        private readonly ICqrsClient _clientApi;
        private int _commandIndex = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal CqrsAppSession(ICqrsClient clientApi, ILoggedInUserInfo user, TAppModel model)
        {
            _resultByCommandIndex = new ConcurrentDictionary<int, CommandResult>();
            _clientApi = clientApi;

            User = user;
            Model = model;

            _clientApi.EventsReceived += OnEventsReceived;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILoggedInUserInfo User { get; private set; }
        public TAppModel Model { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void SendCommand(IServerCommand command, Action<CommandResult> onCompleted, TimeSpan? timeout = null)
        {
            command.Index = Interlocked.Increment(ref _commandIndex);

            if (onCompleted != null)
            {
                _resultByCommandIndex.TryAdd(command.Index, new CommandResult(command, onCompleted));
            }

            _clientApi.SendCommands(new[] { command });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnEventsReceived(IList<IPushEvent> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var completionEvent = events[i] as ICommandProcessedEvent;

                if (completionEvent != null)
                {
                    CommandResult result;

                    if (_resultByCommandIndex.TryRemove(completionEvent.CommandIndex, out result))
                    {
                        Execute.BeginOnUIThread(() => result.NotifyCompleted(completionEvent));
                    }
                }
            }
        }
    }
}
