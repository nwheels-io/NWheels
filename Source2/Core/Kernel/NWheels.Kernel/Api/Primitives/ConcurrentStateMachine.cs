using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NWheels.Kernel.Api.Primitives
{
    public class ConcurrentStateMachine<TState, TTrigger> : StateMachine<TState, TTrigger>
    {
        private readonly BlockingCollection<(TTrigger Trigger, object Context)> _queuedTriggers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConcurrentStateMachine(IStateMachineCodeBehind<TState, TTrigger> codeBehind, ILogger logger) 
            : base(codeBehind, logger)
        {
            _queuedTriggers = new BlockingCollection<(TTrigger trigger, object context)>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunOnCurrentThread(
            Func<ConcurrentStateMachine<TState, TTrigger>, bool> exitWhen,
            Func<Exception, bool> onError = null)
        {
            RunOnCurrentThread(exitWhen, onError, CancellationToken.None);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunOnCurrentThread(
            Func<ConcurrentStateMachine<TState, TTrigger>, bool> exitWhen, 
            Func<Exception, bool> onError,
            CancellationToken cancellation)
        {
            do
            {
                try
                {
                    var dequeued = _queuedTriggers.Take(cancellation);
                    ReceiveTrigger(dequeued.Trigger, dequeued.Context);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    var shouldContinue = !cancellation.IsCancellationRequested;

                    if (onError != null)
                    {
                        shouldContinue &= onError(e);
                    }

                    if (!shouldContinue)
                    {
                        break;
                    }
                }
            } while (!exitWhen(this));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void QueueTrigger(TTrigger trigger)
        {
            _queuedTriggers.Add((trigger, null));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void QueueTrigger(TTrigger trigger, object context)
        {
            _queuedTriggers.Add((trigger, context));
        }
    }
}
