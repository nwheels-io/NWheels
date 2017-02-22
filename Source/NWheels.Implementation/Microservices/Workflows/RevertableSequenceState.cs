namespace NWheels.Microservices.Workflows
{
    public enum RevertableSequenceState
    {
        NotPerformed,
        Performed,
        Reverted,
        RevertFailed
    }
}
