namespace NWheels.Orchestration
{
    public interface IRevertableSequence
    {
        void Perform();
        void Revert();
        RevertableSequenceState State { get; }
    }
}
