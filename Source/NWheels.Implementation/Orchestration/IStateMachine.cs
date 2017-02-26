namespace NWheels.Microservices.Orchestration
{
    public interface IStateMachine<TState, TTrigger>
    {
        void ReceiveTrigger(TTrigger trigger);
        TState CurrentState { get; }
    }
}
