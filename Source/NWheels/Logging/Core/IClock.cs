namespace NWheels.Logging.Core
{
    internal interface IClock
    {
        long ElapsedMilliseconds { get; }
        long ElapsedMicroseconds { get; }
    }
}
