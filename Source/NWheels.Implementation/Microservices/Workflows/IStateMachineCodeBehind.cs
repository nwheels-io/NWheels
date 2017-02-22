namespace NWheels.Microservices.Workflows
{
    public interface IStateMachineCodeBehind<TState, TTrigger>
    {
        void BuildStateMachine(IStateMachineBuilder<TState, TTrigger> machine);
    }
}
