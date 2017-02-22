namespace NWheels.Microservices.Workflows
{
    public interface IRevertableSequence
    {
        void Perform();
        void Revert();
        RevertableSequenceState State { get; }
    }
}
