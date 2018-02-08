namespace NWheels.Kernel.Api.Primitives
{
    public interface IRevertableSequence
    {
        void Perform();
        void Revert();
        RevertableSequenceState State { get; }
    }
}
