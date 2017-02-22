namespace NWheels.Microservices.Workflows
{
    public interface IStateMachine<TState, TTrigger>
    {
        void ReceiveTrigger(TTrigger trigger);
        TState CurrentState { get; }
    }
}
