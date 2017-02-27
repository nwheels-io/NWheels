namespace NWheels.Orchestration
{
    public interface IRevertableSequenceCodeBehind
    {
        void BuildSequence(IRevertableSequenceBuilder sequence);
    }
}
