namespace NWheels.Microservices.Orchestration
{
    public interface IRevertableSequenceCodeBehind
    {
        void BuildSequence(IRevertableSequenceBuilder sequence);
    }
}
