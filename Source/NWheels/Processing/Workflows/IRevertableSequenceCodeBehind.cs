namespace NWheels.Processing.Workflows
{
    public interface IRevertableSequenceCodeBehind
    {
        void BuildSequence(IRevertableSequenceBuilder sequence);
    }
}
