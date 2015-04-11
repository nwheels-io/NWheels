namespace NWheels.Logging.Core
{
    internal interface IThreadLogAnchor
    {
        ThreadLog CurrentThreadLog { get; set; }
    }
}
