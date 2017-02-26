namespace NWheels.Microservices.Orchestration
{
    public interface IRevertableSequence
    {
        void Perform();
        void Revert();
        RevertableSequenceState State { get; }
    }
}
