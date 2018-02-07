using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NWheels.Kernel.Api.Primitives
{
    public static class StateMachineScheduler
    {
        public static StateMachineScheduler<TState, TTrigger> CreateFrom<TState, TTrigger>(StateMachine<TState, TTrigger> stateMachine)
        {
            return new StateMachineScheduler<TState, TTrigger>(stateMachine);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class StateMachineScheduler<TState, TTrigger>
    {
        private readonly StateMachine<TState, TTrigger> _stateMachine;
        private readonly BlockingCollection<(TTrigger Trigger, object Context)> _queuedTriggers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineScheduler(StateMachine<TState, TTrigger> stateMachine)
        {
            _stateMachine = stateMachine;
            _queuedTriggers = new BlockingCollection<(TTrigger trigger, object context)>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _queuedTriggers.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TState RunOnCurrentThread(
            Func<TState, bool> exitWhen,
            Func<TState, Exception, bool> onError = null)
        {
            return RunOnCurrentThread(exitWhen, onError, CancellationToken.None);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TState RunOnCurrentThread(
            Func<TState, bool> exitWhen, 
            Func<TState, Exception, bool> onError,
            CancellationToken cancellation)
        {
            do
            {
                try
                {
                    var dequeued = _queuedTriggers.Take(cancellation);
                    _stateMachine.ReceiveTrigger(dequeued.Trigger, dequeued.Context);
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
                        shouldContinue &= onError(_stateMachine.CurrentState, e);
                    }

                    if (!shouldContinue)
                    {
                        break;
                    }
                }
            } while (!exitWhen(_stateMachine.CurrentState));

            return _stateMachine.CurrentState;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TState CurrentState
        {
            get
            {
                return _stateMachine.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler CurrentStateChanged
        {
            add
            {
                _stateMachine.CurrentStateChanged += value;
            }
            remove
            {
                _stateMachine.CurrentStateChanged -= value;
            }
        }
    }
}
