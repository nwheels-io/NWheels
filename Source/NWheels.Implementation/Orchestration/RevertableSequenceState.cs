namespace NWheels.Microservices.Orchestration
{
    public enum RevertableSequenceState
    {
        NotPerformed,
        Performed,
        Reverted,
        RevertFailed
    }
}
