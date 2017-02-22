namespace NWheels.Microservices.Workflows
{
    public interface IRevertableSequenceCodeBehind
    {
        void BuildSequence(IRevertableSequenceBuilder sequence);
    }
}
