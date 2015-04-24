using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IStateMachineBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> State(TState value);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStateMachineStateBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> SetAsInitial();
        IStateMachineStateBuilder<TState, TTrigger> OnEntered(EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler);
        IStateMachineStateBuilder<TState, TTrigger> OnTimeout(TimeSpan timeout, EventHandler<StateMachineFeedbackEventArgs<TState, TTrigger>> handler, bool recurring = false);
        IStateMachineStateBuilder<TState, TTrigger> OnLeaving(EventHandler<StateMachineEventArgs<TState, TTrigger>> handler);
        IStateMachineTransitionBuilder<TState, TTrigger> OnTrigger(TTrigger trigger);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStateMachineTransitionBuilder<TState, TTrigger>
    {
        IStateMachineStateBuilder<TState, TTrigger> TransitionTo(
            TState destination, 
            EventHandler<StateMachineEventArgs<TState, TTrigger>> onTransitioning = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class StateMachineEventArgs<TState, TTrigger> : EventArgs
    {
        public StateMachineEventArgs(TState initialState)
        {
            HasToState = true;
            ToState = initialState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineEventArgs(TState from, TState to, TTrigger trigger, object context)
        {
            HasFromState = true;
            HasToState = true;
            HasTrigger = true;

            FromState = from;
            ToState = to;
            Trigger = trigger;
            Context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasFromState { get; private set; }
        public bool HasToState { get; private set; }
        public bool HasTrigger { get; private set; }
        public TState FromState { get; private set; }
        public TState ToState { get; private set; }
        public TTrigger Trigger { get; private set; }
        public object Context { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class StateMachineFeedbackEventArgs<TState, TTrigger> : StateMachineEventArgs<TState, TTrigger>
    {
        public StateMachineFeedbackEventArgs(TState initialState)
            : base(initialState)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public StateMachineFeedbackEventArgs(TState from, TState to, TTrigger trigger, object context)
            : base(from, to, trigger, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReceiveFeedack(TTrigger feedback)
        {
            this.HasFeedback = true;
            this.Feedback = feedback;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasFeedback { get; private set; }
        public TTrigger Feedback { get; private set; }
    }
}
