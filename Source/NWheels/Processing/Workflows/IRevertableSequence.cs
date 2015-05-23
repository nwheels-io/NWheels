using NWheels.Processing.Workflows.Core;

namespace NWheels.Processing.Workflows
{
    public interface IRevertableSequence
    {
        void Perform();
        void Revert();
        RevertableSequenceState State { get; }
    }
}
